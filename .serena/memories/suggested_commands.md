# Suggested Commands

No CLI build pipeline; compilation and play-mode testing are done inside the Unity Editor.

## Git (PowerShell on Windows)
```powershell
git status
git add <files>
git commit -m "message"
```

## File search (PowerShell)
```powershell
Get-ChildItem -Path "Assets/Game/0_Scripts" -Recurse -Filter "*.cs" | Select-Object Name
```

## Opening Unity project
Open Unity Hub → select project at `D:\Game_Projects\3th_grade\Graduation-project\GameProject`.
