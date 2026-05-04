# Unity MWA SDK — Test-host project

This Unity project is the Gate 5 test-host for the `com.solana.unity_sdk` package.
It is deliberately located under `Tests~/` (tilde-suffix) so Unity Package Manager
strips it out when the package is imported by a consumer — tests never leak into
consumer `Assets/`.

## What this runs

Two EditMode tests live under `Assets/Tests/EditMode/`:

| Test | File | Proves |
|------|------|--------|
| `TestHostBootstrapTest.AdapterAssemblyLoads` | `TestHostBootstrapTest.cs` | The runtime assembly `com.solana.unity_sdk` compiles and is visible to the test-host (ensures the `IsExternalInit` polyfill + `LangVersion 9` + `Nullable enable` stack is healthy end-to-end). |
| `UpmCleanImportTest.NoTestAssetLeakage` | `UpmCleanImportTest.cs` | `Tests~/` exists at the package root and `Assets/Tests~` does NOT appear in the package source (UPM-exclusion convention). |

Both tests must exit `Passed` on every Gate 5 run. Story 1-1 ACs #1 and #2 (AC-A-7) depend on this.

## CLI invocation

### macOS

```bash
/Applications/Unity/Hub/Editor/2022.3.62f3/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics \
  -projectPath Tests~/TestHost \
  -runTests -testPlatform EditMode \
  -testResults TestResults.xml \
  -logFile -
```

Run from the repo root (so `Tests~/TestHost` resolves correctly).

### Windows

```cmd
"C:/Program Files/Unity/Hub/Editor/2022.3.62f3/Editor/Unity.exe" ^
  -batchmode -nographics ^
  -projectPath Tests~/TestHost ^
  -runTests -testPlatform EditMode ^
  -testResults TestResults.xml ^
  -logFile -
```

### Flag reference

| Flag | Purpose |
|------|---------|
| `-batchmode` | No Editor UI; required for CI. |
| `-nographics` | No GPU context; matches headless CI runners. |
| `-projectPath Tests~/TestHost` | Opens this test-host project. Relative path resolved from the caller's working directory. |
| `-runTests` | Runs the test runner immediately after project load. |
| `-testPlatform EditMode` | EditMode only for Story 1-1; PlayMode tests may land in later stories. |
| `-testResults TestResults.xml` | Writes the result report. |
| `-logFile -` | Streams the Unity log to stdout. |

## Contracts

### Exit code

- `0` — all tests passed.
- Any non-zero value — at least one test failed, Unity could not open the project, the test assembly did not compile, or a package could not be resolved.

CI MUST treat any non-zero exit as a Gate 5 failure.

### `TestResults.xml` format

Unity 2022.3 emits **NUnit 3 XML** (JUnit-compatible). Key elements:

- Root: `<test-run id="..." testcasecount="..." result="...">` where `result="Passed"` means the whole run passed.
- `<test-case name="SolanaMobileStack.Tests.EditMode.TestHostBootstrapTest.AdapterAssemblyLoads" result="Passed">` must be present.
- `<test-case name="SolanaMobileStack.Tests.EditMode.UpmCleanImportTest.NoTestAssetLeakage" result="Passed">` must be present.

A quick sanity check after a run:

```bash
# Each sentinel must appear with result="Passed". Both must match; zero `result="Failed"` entries.
grep -F 'AdapterAssemblyLoads"' TestResults.xml | grep -Fq 'result="Passed"' && echo "bootstrap=Passed"
grep -F 'NoTestAssetLeakage"'   TestResults.xml | grep -Fq 'result="Passed"' && echo "upm-import=Passed"
grep -c 'result="Failed"'       TestResults.xml   # must be 0
```

### First-run caveat

The first batchmode invocation on a fresh clone takes longer — Unity resolves `com.unity.test-framework@1.1.33` and caches the package. Subsequent runs skip resolution and finish in roughly 30-60 s on current hardware.

### Asset import warnings are expected

Unity logs a handful of `[AssetImportWorker]` warnings the first time a project is opened. They are informational and do not affect the exit code. Only `error CS*`, `Assertion failed`, or test `Failed` results indicate a real problem.

## Gate 5 consumption pattern (future CI hook)

When Gate 5 verification runs, the harness is expected to:

1. Invoke the appropriate CLI above.
2. Check exit code.
3. Parse `TestResults.xml` for `<test-case ... result="Failed">` — zero entries required.
4. Assert both sentinel test names appear with `result="Passed"`.
5. Archive `TestResults.xml` with the Gate 5 artifact bundle for audit.

GitHub Actions / GitLab CI / Jenkins xUnit plugin all consume NUnit 3 XML without transformation.

## Related docs

- `docs/plan.md` — Story 1-1 acceptance criteria in full.
- `docs/architecture.md` §13 — testing strategy + Test-host layout rationale (DD-7, DD-19).
- `docs/test-strategy.md` — entry/exit criteria inherited by every subsequent story.
- `docs/test-cases.md` — supplementary TCs including `TC-W1-01`, `TC-W1-02`, `TC-W1-03` (test-host isolation / polyfill conflict / manifest integrity).
