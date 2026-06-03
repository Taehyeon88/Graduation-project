# Task Completion

No CLI linter, formatter, or test runner. Completion steps are Unity Editor-based:

1. **Save all modified `.cs` files** — Unity auto-detects and recompiles.
2. **Check Unity Console** for compilation errors (no red errors = compilable).
3. **Enter Play Mode** in the `GameDemoScene` to verify runtime behavior.
4. **Git commit** with descriptive message following project convention:
   - Format: `[Tag] 설명` (Korean description)
   - Tags seen: `[Pro Fix]`, `[Pro Feat]`, `[Clean]`, `[BGM]`
