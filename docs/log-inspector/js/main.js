// read text from clipboard
async function pasteResult() {
  try {
    const text = await navigator.clipboard.readText();
    if (!text) {
      alert("Clipboard is empty");
      return;
    }
    document.getElementById("logInput").value = text;
    analyzeLog(); // direkt analysieren
  } catch (err) {
    alert("No access to clipboard " + err);
  }
}

// Analyze the pasted log and extract issues, registry keys, and plugin info
function analyzeLog() {
  const log = document.getElementById("logInput").value;
  const output = document.getElementById("output");
  const lines = log.split("\n");

  const issues = lines.filter((line) => line.startsWith("❌"));
  const regKeys = lines.filter((line) => line.includes("HKEY_") || line.includes("➤"));
  const plugins = lines.filter((line) => line.match(/Plugin ready: .*\.ps1/i));

  output.innerHTML = `
    <h3>🧪 Found ${issues.length} issues</h3>
    <div>${issues.map((i) => `<div class="issue">${i}</div>`).join("")}</div>
    <hr>
    <h3>🗂 Registry Keys</h3>
    <div>${regKeys.map((k) => `<div class="key">${k}</div>`).join("")}</div>
    <hr>
    <h3>📦 Loaded Plugins</h3>
    <div>${plugins.map((p) => `<div class="plugin">${p}</div>`).join("")}</div>
  `;
}

// Save screenshot of the output section
function captureResult() {
  html2canvas(document.getElementById("output")).then((canvas) => {
    const link = document.createElement("a");
    link.download = "Winslop-results.png";
    link.href = canvas.toDataURL();
    link.click();
  });
}

// Native share (for mobile browsers)
function shareResult() {
  const text = document.getElementById("output").innerText;
  if (navigator.share) {
    navigator
      .share({
        title: "Winslop Analysis Results",
        text: text,
      })
      .catch((err) => console.log("Share failed:", err));
  } else {
    alert("Sharing is not supported by your browser.");
  }
}

// Translation
function translateViaGoogle() {
  const text = document.getElementById("logInput").value || "";
  if (!text.trim()) {
    alert("Paste a log first.");
    return;
  }

  // target : auto, de-De etc.
  const url =
    "https://translate.google.com/?sl=auto&tl=de&op=translate&text=" +
    encodeURIComponent(text);

  window.open(url, "_blank", "noopener");
}


// Share the result as an image on Twitter/X
function shareOnTwitter() {
  const outputEl = document.getElementById("output");
  if (!outputEl.innerText.trim()) return alert("No results to share yet.");

  // Create screenshot from result div
  html2canvas(outputEl).then((canvas) => {
    const dataUrl = canvas.toDataURL("image/png");

    // Convert image to base64 string (can't upload directly to Twitter)
    // Instead, show preview and user uploads manually via prompt
    const win = window.open();
    win.document.write(`<h2>📷 Screenshot ready for X / Twitter</h2>`);
    win.document.write(
      `<p>Right-click the image below and save it to upload on Twitter manually.</p>`
    );
    win.document.write(
      `<img src="${dataUrl}" style="max-width:100%;border:1px solid #ccc;" />`
    );
    win.document.write(
      `<p><a href="https://twitter.com/intent/tweet?text=Check%20out%20my%20Winslop%20analysis!&hashtags=Winslop,loganalyzer" target="_blank">➡️ Click here to post on X</a></p>`
    );
  });
}
