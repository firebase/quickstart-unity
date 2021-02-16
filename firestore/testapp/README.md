## Setting up Unity Test Framework for the test app

Follow these steps to setup Firestore integration tests using Unity Test
Framework, which is based on NUnit.

*   Install "Test Framework" from the package manager: go to "Window -> Package
    Manager" and search for "Test Framework".

*   Go to menu "Window -> General -> Test Runner" if have not already, drag the
    new window to your Unity Editor's main panel.

*   Above step might be enough to pull all tests automatically. If not, go to
    "Project" panel, click on "Assets -> Tests -> Tests" to bring up "Inspector"
    panel, tick some options then "apply". This should force the Editor to load
    up all tests.

## Running unit tests from Unity Editor

Once all tests are loaded, you can run any tests by right clicking it to bring
up context menu, and run from there. There is also a "Run all in player" button,
which starts a standalone app for the target platform and run the tests inside
the app.

## Running unit tests from Command Line

One big benefit of using test framework is we are not able to run all the tests
from command line, with a headless Unity editor.

Example command line:

```bash
# Assuming 2019.4.9f1 is used, and the testapp is under `~/projects`.
/Applications/2019.4.9f1/Unity.app/Contents/MacOS/Unity -runTests -batchmode -projectPath ~/projects/unity/testapp -testResults ~/tmp/results.xml -testPlatform StandaloneOSX
```

See
[here](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/reference-command-line.html)
for the detailed options.

This opens up potentials of some CI pipeline, once we break out of google3.
