## Quick start

1. Open **Winslop.exe**
2. Use the **tabs** to switch between areas:
   - **Windows**: system tweaks (feature tree)
   - **Applications**: app scan/uninstall (if enabled in your build)
   - **Extensions**: additional modules/extensions

3. Use **Inspect system** to scan/analyze.
4. Select the tweaks you want (checkboxes).
5. Use **Apply selected changes** to apply the checked items.

---

## UI overview

### Top bar
- **Menu button (☰)**  
  Opens the main menu (selection actions, import/export, plugins, etc. depending on your build).
- **Search**  
  Filters the current view (e.g., feature tree).  
  Tip: clear the search to show all items again.
- **Heart button (❤)**  
  About this app and versioning information

### Tabs
- **Windows**  
  Shows the feature/plugin tree. Items are grouped (e.g., *Issues*, *System*, *MS Edge*, *Privacy*…).
- **Applications**  
  Scans installed apps and allows uninstalling selected ones (if present).
- **Extensions**  
  Hosts extension views/modules.

### Feature tree (Windows tab)
- Checkboxes represent individual tweaks.  
- Checking a parent node typically checks/unchecks all children.
- Right-click a node for context actions (analyze/fix/restore/help) if available.

### Feature tree context menu (right-click)

Right-click any node in the **Windows** feature tree to open the context menu:

- **Analyze**  
  Analyzes only the selected node (or the whole group if you clicked a parent node).  
  Useful to quickly check the current state of a single tweak/plugin.

- **Fix**  
  Applies the selected node (or all checked items under a parent node).  
  This is basically a “run only this item” action.

- **Restore**  
  Reverts the selected node back to its original/default state (if supported).  
  For plugins, this uses the plugin’s undo/restore command when available.

- **Help (F1)**  
  Shows help/details for the selected node (what it does and any notes/warnings).


### Action selector (dropdown)
> [!TIP]
> **Online Log Inspector:** You can inspect the full log in a clean, readable view via the **Actions** dropdown menu → **Log Inspector**.

- **Select an action…**  
  A list of available log actions / helpers (depends on your build).  
  Typically used to perform a specific operation or change the logging behavior.

### Main buttons (bottom)
- **Inspect system**  
  Scans/analyzes the current tab content (e.g., detects system state, checks plugins, scans apps).
- **Apply selected changes**  
  Applies all currently checked items in the active tab (e.g., apply tweaks / uninstall selected apps).
---

## Selection import/export (optional)

If your build includes selection transfer:
- You can export your current checked items to a simple `.sel` text file.
- You can import a `.sel` file to restore a selection quickly.

Some builds also support auto-loading `selection.sel` from the same folder as the executable (opt-in prompt).

---

## Notes / Safety

This tool changes Windows settings. Use **Inspect** first, review what is checked, then **Apply**.
Some changes may require sign-out or restart to fully take effect.
