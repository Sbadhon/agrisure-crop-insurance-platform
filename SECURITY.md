# Security policy

This repository contains synthetic demonstration data only.

Do not use the header-based actor mechanism in a public production deployment. Replace it with validated OIDC/JWT authentication, strip caller-supplied identity headers at the gateway, and configure secrets outside source control.

To report a vulnerability, open a private GitHub security advisory rather than a public issue.
