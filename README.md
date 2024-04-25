# Unity Guard - Safecy

Welcome to Unity Guard, a security-focused Unity package designed to help you protect your Unity projects from malicious assets and scripts. This package scans imported and updated assets, detects potentially harmful code patterns, and provides early warnings to prevent security risks in your projects.

## Features

- **Malicious Asset Detection**: Scans imported assets to check for suspicious or harmful content.
- **Custom Asset Post-Processing**: Automatically deletes assets flagged as malicious.
- **Basic Content Analysis**: Identifies potentially dangerous code patterns in scripts.
- **Size and Type Checks**: Warns about unusually large scripts and unknown asset types.
- **Prefab Safety**: Detects executable code in non-script assets.
- **Malicious URL Detection**: Checks assets for known malicious URLs.

## Installation

1. Ensure your Unity version is 2019.4 or later.
2. Open your Unity project.
3. In the Unity Editor, go to **Window > Package Manager**.
4. Click the `+` icon in the top-left corner and select **Add package from git URL...**.
5. Enter the following URL: `https://github.com/safecy/unity-guard.git`
6. Click **Add** to import the package.

## Usage

Once the package is installed, it automatically scans imported and updated assets in your project. You don't need to configure or call any additional methods; the scanning process is integrated into Unity's asset import pipeline.

### Detecting Malicious Assets

- If a malicious asset is detected during import, you will see an error message in the Unity Console, indicating the file that caused the issue.
- Unity Guard automatically deletes malicious assets, providing a secure environment for your project.

### Logging and Alerts

- Warnings and error messages are logged to the Unity Console, allowing you to review detected issues.
- Warnings indicate potentially suspicious assets or scripts that require further investigation.
- Errors occur when malicious assets are detected and deleted from your project.

## Customization

To customize the scanning process, you can edit the `MaliciousAssetsScanner` class in the `UnityGuard` namespace. Be cautious when modifying this class, as incorrect changes may affect the package's effectiveness in detecting security risks.

## Troubleshooting

- **False Positives**: If Unity Guard flags an asset incorrectly, ensure that it does not contain suspicious code patterns or unusual content. Adjust the detection logic in `MaliciousAssetsScanner` as needed.
- **Failed Asset Import**: If an asset import fails, check the Unity Console for error messages and investigate the cause.
- **Asset Not Found**: This warning may indicate a missing or tampered asset. Verify the integrity of your project files.

## License

Unity Guard is open-source software released under the MIT License. You are free to use, modify, and distribute this package. See the [LICENSE](LICENSE) file for details.

## Contact and Support

If you have questions, suggestions, or need support, contact the author at [pyyupsk@proton.me](mailto:pyyupsk@proton.me). For more information, visit the [GitHub repository](https://github.com/safecy).

Thank you for using Unity Guard! We hope this package helps you maintain a secure and safe Unity development environment.