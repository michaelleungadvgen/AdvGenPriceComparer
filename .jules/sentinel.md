## 2025-05-01 - Unverified Update File Download Execution
**Vulnerability:** Update service downloaded MSI installers and executed them directly without cryptographically verifying the file hash (`FileHash` in the manifest was ignored), leading to arbitrary code execution if the update source is compromised or intercepted.
**Learning:** File hashing must happen in-memory immediately after the download and BEFORE writing to disk to prevent Time-of-Check to Time-of-Use (TOCTOU) file replacement attacks.
**Prevention:** Always verify downloaded installer or executable files against a known-good checksum before storing or executing them.
