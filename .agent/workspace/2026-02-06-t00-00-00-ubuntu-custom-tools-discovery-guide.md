# Discovering Custom Tools on Ubuntu: A Comprehensive Guide

**Date:** 2026-02-06  
**Scope:** How to list all custom tools on Ubuntu and differentiate them from a stock Ubuntu image

---

## Executive Summary

Discovering all custom tools on an Ubuntu system requires checking multiple package managers, non-package installation methods, and custom installation locations. Tools can be installed via apt/dpkg, snap, flatpak, npm/yarn/pnpm/bun, cargo, pipx, and manual curl/wget scripts. To diff from stock Ubuntu, compare against the official package manifests published by Canonical.

---

## 1. Package Manager-Based Tools

### 1.1 APT/DPKG (Traditional Packages)

These are the standard Ubuntu packages managed by the system package manager.

```bash
# List all installed packages with details
dpkg -l

# List only package names (cleaner output)
dpkg -l | grep '^ii' | awk '{print $2}'

# Alternative using apt
apt list --installed

# Count total installed packages
dpkg -l | grep '^ii' | wc -l

# Export to file for comparison
dpkg -l > installed-packages.txt
```

**Key locations:**
- Binary files: `/usr/bin/`, `/bin/`
- Libraries: `/usr/lib/`, `/lib/`
- Documentation: `/usr/share/doc/`

### 1.2 Snap Packages

Snap is Ubuntu's universal packaging format with sandboxed applications.

```bash
# List all installed snaps
snap list

# List with more details
snap list --all

# Show snap services
snap services

# Export to file
snap list > installed-snaps.txt
```

**Key locations:**
- Snap files: `/snap/` or `/var/snap/`
- User data: `~/snap/`

### 1.3 Flatpak Packages

Flatpak provides cross-distribution sandboxed applications.

```bash
# List all installed flatpaks
flatpak list

# List by application ID only
flatpak list --app --columns=application

# Show runtimes and apps
flatpak list --app --runtime

# Export to file
flatpak list > installed-flatpaks.txt
```

**Key locations:**
- System apps: `/var/lib/flatpak/app/`
- User apps: `~/.local/share/flatpak/app/`

---

## 2. Non-Package Manager Tools

### 2.1 Node.js/NPM Global Packages

Tools installed via npm/yarn/pnpm globally.

```bash
# NPM global packages
npm list -g --depth=0

# NPM global packages with versions (full tree)
npm list -g

# Get global prefix location
npm config get prefix

# Yarn global packages (if installed)
yarn global list

# Pnpm global packages (if installed)
pnpm list -g --depth=0

# Export NPM globals
npm list -g --depth=0 > npm-global-packages.txt
```

**Key locations:**
- Global binaries: `/usr/local/bin/` (npm) or `~/.local/bin/`
- Global node_modules: `/usr/local/lib/node_modules/` or `~/.local/lib/node_modules/`

### 2.2 Bun Runtime Packages

Bun is a fast JavaScript runtime and package manager (alternative to Node.js).

```bash
# List local project packages (top-level only)
bun pm ls

# List all packages including transitive dependencies
bun pm ls -a

# List global packages (if bun supports it in your version)
bun pm ls -g 2>/dev/null || ls ~/.bun/install/global/node_modules/

# Show bun binary paths
bun pm bin          # Local project bin
bun pm bin -g       # Global bin directory

# Show bun cache location
bun pm cache

# Clear bun cache
bun pm cache rm

# Check bun version and runtime info
bun --version
bun --revision

# Export bun packages
bun pm ls > bun-packages.txt
```

**Key locations:**
- Bun binaries: `~/.bun/bin/`
- Global packages: `~/.bun/install/global/node_modules/`
- Global bin: `~/.bun/bin/`
- Cache: `~/.bun/install/cache/`
- Configuration: `bunfig.toml` (project) or `~/.bunfig.toml` (global)

**Note:** As of early 2025, Bun's global package listing is still evolving. Check `~/.bun/install/global/node_modules/` directly or use `ls -la ~/.bun/bin/` for globally installed binaries.

### 2.3 Rust/Cargo Installed Binaries

Tools installed via `cargo install`.

```bash
# List all installed cargo packages
# Shows package name, version, and associated binaries
cargo install --list

# Alternative: list files in cargo bin directory
ls ~/.cargo/bin/

# Check cargo configuration
cat ~/.cargo/.crates.toml
cat ~/.cargo/.crates2.json | jq '.'

# Third-party tool for updates (if installed)
cargo-binlist --list
cargo-list list

# Export cargo packages
cargo install --list > cargo-packages.txt
```

**Key locations:**
- Binaries: `~/.cargo/bin/` (or `$CARGO_HOME/bin/`)
- Registry cache: `~/.cargo/registry/`

### 2.3 Python/Pipx Installed Tools

Tools installed via pipx (isolated Python applications).

```bash
# List all pipx installed applications
pipx list

# Short format (names only)
pipx list --short

# JSON format for parsing
pipx list --json

# List with injected packages
pipx list --include-injected

# User pip packages (if using pip --user)
pip list --user

# Show pipx environment
pipx environment

# Export pipx packages
pipx list > pipx-packages.txt
```

**Key locations:**
- Virtual environments: `~/.local/share/pipx/venvs/`
- Exposed binaries: `~/.local/bin/`

### 2.4 Go Installed Tools

Tools installed via `go install`.

```bash
# List installed go binaries
go list -m all 2>/dev/null || ls $(go env GOPATH)/bin/

# GOPATH bin location
echo $(go env GOPATH)/bin
ls $(go env GOPATH)/bin/

# GOBIN location (if set)
echo $GOBIN
ls $GOBIN 2>/dev/null || echo "GOBIN not set"
```

**Key locations:**
- Binaries: `$(go env GOPATH)/bin/` (default: `~/go/bin/`)
- Source: `$(go env GOPATH)/src/`

### 2.5 .NET Global Tools

.NET CLI tools installed via `dotnet tool install`.

```bash
# List all installed .NET global tools
dotnet tool list -g

# List local tools (in current directory)
dotnet tool list

# Check if dotnet is installed and show version
dotnet --version
dotnet --info

# List tool paths
echo "Global tools location:"
ls -la ~/.dotnet/tools/

# Export .NET tools
dotnet tool list -g > dotnet-tools.txt
```

**Key locations:**
- Global tools: `~/.dotnet/tools/` (or `$DOTNET_TOOLS/` if set)
- Local tools: `.config/dotnet-tools.json` (project-specific)
- Tool cache: `~/.nuget/packages/`
- Tool manifests: `~/.dotnet/toolResolverCache/`

**Note:** .NET tools can be installed globally (available everywhere) or locally (project-specific). Use `-g` or `--global` flag for global installation.

---

## 3. Manual/Script-Based Installations

### 3.1 Common Custom Installation Directories

Many tools install to these locations outside of package managers:

```bash
# /usr/local (system-wide custom installs)
ls -la /usr/local/bin/
ls -la /usr/local/lib/
ls -la /usr/local/share/
ls -la /usr/local/opt/  # Homebrew on Linux (Linuxbrew)

# /opt (large third-party applications)
ls -la /opt/

# User-local installs
ls -la ~/.local/bin/
ls -la ~/.local/lib/
ls -la ~/.local/share/

# Home directories
ls -la ~/.bin/ 2>/dev/null || echo "~/.bin not found"
ls -la ~/bin/ 2>/dev/null || echo "~/bin not found"
ls -la ~/.dotfiles/bin/ 2>/dev/null || echo "~/.dotfiles/bin not found"

# Application-specific
ls -la ~/.dotnet/tools/ 2>/dev/null || echo ".NET tools not found"
ls -la ~/.pulumi/bin/ 2>/dev/null || echo "Pulumi not found"
ls -la ~/.terraform.d/ 2>/dev/null || echo "Terraform plugins not found"
```

### 3.2 Shell Script Install Detection

Find tools installed via curl/wget scripts:

```bash
# Search for common installer patterns in command history
history | grep -E '(curl.*install|wget.*install|curl.*sh|wget.*sh)' | tail -20

# Check for install scripts in common locations
find ~ -maxdepth 2 -name "install*.sh" -o -name "setup*.sh" 2>/dev/null

# Check /tmp for installer remnants
ls -la /tmp/ | grep -E '(install|setup|curl)'

# Look at recently modified files in /usr/local
find /usr/local/bin -type f -mtime -30 2>/dev/null | head -20
```

### 3.3 Container/Virtualization Tools

```bash
# Docker images and tools
docker images --format "table {{.Repository}}:{{.Tag}}\t{{.Size}}" 2>/dev/null || echo "Docker not available"
docker ps -a --format "table {{.Names}}\t{{.Image}}\t{{.Status}}" 2>/dev/null

# Podman
podman images 2>/dev/null || echo "Podman not available"

# Kubernetes tools
kubectl version --client 2>/dev/null || echo "kubectl not installed"
helm version --client 2>/dev/null || echo "Helm not installed"
minikube version 2>/dev/null || echo "Minikube not installed"
```

---

## 4. Environment and PATH Analysis

### 4.1 Check PATH Components

```bash
# Show current PATH
echo $PATH | tr ':' '\n'

# Find all executables in PATH
# This can be slow on large systems
for dir in $(echo $PATH | tr ':' ' '); do
    if [ -d "$dir" ]; then
        echo "=== $dir ===" 
        ls "$dir" 2>/dev/null
    fi
done

# Quick count of executables in each PATH dir
for dir in $(echo $PATH | tr ':' ' '); do
    if [ -d "$dir" ]; then
        count=$(find "$dir" -maxdepth 1 -type f -executable 2>/dev/null | wc -l)
        echo "$dir: $count executables"
    fi
done
```

### 4.2 Shell-Specific Tools

```bash
# Check shell rc files for tool installations
grep -E '(export PATH|alias|eval)' ~/.bashrc ~/.bash_profile ~/.zshrc ~/.zsh_profile 2>/dev/null | head -30

# Check for version managers
echo "=== Version Managers ==="
which rbenv rvm nvm n pyenv jenv 2>/dev/null

# List available versions (if managers are installed)
rbenv versions 2>/dev/null || echo "rbenv not active"
nvm list 2>/dev/null || echo "nvm not active"
pyenv versions 2>/dev/null || echo "pyenv not active"
```

---

## 5. Diffing from Stock Ubuntu

### 5.1 Get Stock Ubuntu Package Manifest

Canonical publishes package manifests for each Ubuntu image:

```bash
# Download manifest for your Ubuntu version
# Example for Ubuntu 22.04 LTS (Jammy)
UBUNTU_VERSION="jammy"
MANIFEST_URL="https://cloud-images.ubuntu.com/${UBUNTU_VERSION}/current/${UBUNTU_VERSION}-server-cloudimg-amd64.manifest"
wget -O stock-manifest.txt "$MANIFEST_URL"

# Extract package names from manifest
grep -oP '^[a-z0-9\-\+\.]+' stock-manifest.txt | sort -u > stock-packages.txt

# Get current installed packages
dpkg -l | grep '^ii' | awk '{print $2}' | sort -u > current-packages.txt

# Find differences
# Packages not in stock (custom installed)
comm -23 current-packages.txt stock-packages.txt > custom-apt-packages.txt

# Packages removed from stock
comm -13 current-packages.txt stock-packages.txt > removed-packages.txt
```

### 5.2 Automated System Comparison

```bash
#!/bin/bash
# save-as: compare-to-stock.sh

UBUNTU_VERSION=$(lsb_release -cs)
STOCK_URL="https://cloud-images.ubuntu.com/${UBUNTU_VERSION}/current/${UBUNTU_VERSION}-server-cloudimg-amd64.manifest"

echo "Comparing to Ubuntu ${UBUNTU_VERSION} stock image..."

# Download stock manifest
if ! wget -q -O /tmp/stock-manifest.txt "$STOCK_URL" 2>/dev/null; then
    echo "Warning: Could not download manifest for ${UBUNTU_VERSION}"
    echo "Trying to use local /var/lib/apt/extended_states..."
    # Alternative: use apt-mark to find auto-installed packages
    apt-mark showauto | sort -u > /tmp/stock-packages.txt
else
    grep -oP '^[a-z0-9\-\+\.]+' /tmp/stock-manifest.txt | sort -u > /tmp/stock-packages.txt
fi

# Current packages
dpkg -l | grep '^ii' | awk '{print $2}' | sort -u > /tmp/current-packages.txt

echo ""
echo "=== CUSTOM APT PACKAGES (likely manually installed) ==="
comm -23 /tmp/current-packages.txt /tmp/stock-packages.txt

echo ""
echo "=== CUSTOM SNAPS ==="
snap list | grep -v "^Name"

echo ""
echo "=== CUSTOM FLATPAKS ==="
flatpak list --app --columns=application 2>/dev/null || echo "No flatpaks installed"

echo ""
echo "=== /usr/local/bin CONTENTS ==="
ls -la /usr/local/bin/ 2>/dev/null | grep -v "^total" | tail -n +2

echo ""
echo "=== NPM GLOBALS ==="
npm list -g --depth=0 2>/dev/null || echo "No npm global packages"

echo ""
echo "=== CARGO PACKAGES ==="
cargo install --list 2>/dev/null || echo "No cargo packages"

echo ""
echo "=== PIPX PACKAGES ==="
pipx list --short 2>/dev/null || echo "No pipx packages"
```

### 5.3 Using diffoscope for Deep Comparison

For comparing entire system states or container images:

```bash
# Install diffoscope for deep comparison
sudo apt install diffoscope

# Compare two package lists
diffoscope stock-packages.txt current-packages.txt --text diff.txt

# Compare entire directories (useful for /usr/local)
diffoscope /reference/usr/local /usr/local --html diff-report.html
```

### 5.4 Using debsums for Integrity Check

```bash
# Install debsums
sudo apt install debsums

# Check for modified files in installed packages
sudo debsums -c

# Generate manifest of all installed packages with checksums
sudo debsums -a > installed-package-checksums.txt
```

---

## 6. Complete System Inventory Script

```bash
#!/bin/bash
# save-as: system-inventory.sh
# Generates a comprehensive report of all installed tools

OUTPUT_DIR="$HOME/system-inventory-$(date +%Y%m%d)"
mkdir -p "$OUTPUT_DIR"

echo "Generating system inventory in $OUTPUT_DIR..."

# 1. APT/DPKG packages
echo "Collecting APT packages..."
dpkg -l > "$OUTPUT_DIR/apt-packages.txt"
apt list --installed > "$OUTPUT_DIR/apt-installed.txt" 2>/dev/null

# 2. Snaps
echo "Collecting Snaps..."
snap list > "$OUTPUT_DIR/snaps.txt" 2>/dev/null || echo "No snaps installed" > "$OUTPUT_DIR/snaps.txt"

# 3. Flatpaks
echo "Collecting Flatpaks..."
flatpak list > "$OUTPUT_DIR/flatpaks.txt" 2>/dev/null || echo "No flatpaks installed" > "$OUTPUT_DIR/flatpaks.txt"

# 4. NPM globals
echo "Collecting NPM globals..."
npm list -g --depth=0 > "$OUTPUT_DIR/npm-globals.txt" 2>/dev/null || echo "npm not available" > "$OUTPUT_DIR/npm-globals.txt"

# 5. Yarn globals
echo "Collecting Yarn globals..."
yarn global list > "$OUTPUT_DIR/yarn-globals.txt" 2>/dev/null || echo "yarn not available" > "$OUTPUT_DIR/yarn-globals.txt"

# 6. Cargo packages
echo "Collecting Cargo packages..."
cargo install --list > "$OUTPUT_DIR/cargo-packages.txt" 2>/dev/null || echo "cargo not available" > "$OUTPUT_DIR/cargo-packages.txt"

# 7. Pipx packages
echo "Collecting Pipx packages..."
pipx list > "$OUTPUT_DIR/pipx-packages.txt" 2>/dev/null || echo "pipx not available" > "$OUTPUT_DIR/pipx-packages.txt"

# 8. User pip packages
echo "Collecting user pip packages..."
pip list --user > "$OUTPUT_DIR/pip-user-packages.txt" 2>/dev/null || pip3 list --user > "$OUTPUT_DIR/pip-user-packages.txt" 2>/dev/null || echo "pip not available" > "$OUTPUT_DIR/pip-user-packages.txt"

# 9. Custom directories
echo "Collecting custom directory listings..."
ls -la /usr/local/bin/ > "$OUTPUT_DIR/usr-local-bin.txt" 2>/dev/null
ls -la /opt/ > "$OUTPUT_DIR/opt-contents.txt" 2>/dev/null
ls -la ~/.local/bin/ > "$OUTPUT_DIR/user-local-bin.txt" 2>/dev/null
ls -la ~/.cargo/bin/ > "$OUTPUT_DIR/cargo-bin.txt" 2>/dev/null
ls -la ~/.dotnet/tools/ > "$OUTPUT_DIR/dotnet-tools.txt" 2>/dev/null

# 10. PATH analysis
echo "Collecting PATH info..."
echo "$PATH" | tr ':' '\n' > "$OUTPUT_DIR/path-dirs.txt"
for dir in $(echo $PATH | tr ':' ' '); do
    if [ -d "$dir" ]; then
        count=$(find "$dir" -maxdepth 1 -type f -executable 2>/dev/null | wc -l)
        echo "$dir: $count executables" >> "$OUTPUT_DIR/path-executables.txt"
    fi
done

# 11. System info
echo "Collecting system info..."
lsb_release -a > "$OUTPUT_DIR/system-info.txt" 2>/dev/null
uname -a >> "$OUTPUT_DIR/system-info.txt"

# 12. Summary report
echo "Creating summary report..."
cat > "$OUTPUT_DIR/INVENTORY-SUMMARY.txt" << 'EOF'
=============================================
   SYSTEM INVENTORY REPORT
=============================================
Generated: $(date)

CONTENTS:
---------
1. apt-packages.txt       - All dpkg/APT packages
2. apt-installed.txt      - APT installed only
3. snaps.txt              - Installed snap packages
4. flatpaks.txt           - Installed flatpak packages
5. npm-globals.txt        - NPM global packages
6. yarn-globals.txt       - Yarn global packages
7. cargo-packages.txt     - Cargo installed binaries
8. pipx-packages.txt      - Pipx installed applications
9. pip-user-packages.txt - User Python packages

DIRECTORIES SCANNED:
-------------------
10. usr-local-bin.txt     - /usr/local/bin contents
11. opt-contents.txt      - /opt/ contents
12. user-local-bin.txt    - ~/.local/bin/ contents
13. cargo-bin.txt         - ~/.cargo/bin/ contents
14. dotnet-tools.txt      - ~/.dotnet/tools/ contents

SYSTEM INFO:
-----------
15. path-dirs.txt         - PATH directories
16. path-executables.txt  - Executable counts per PATH dir
17. system-info.txt       - OS and kernel information

CUSTOM INSTALL DETECTION:
------------------------
- Check /usr/local/bin/ for non-APT tools
- Check ~/.local/bin/ for user-installed tools
- Check ~/.cargo/bin/ for Rust tools
- Check ~/.dotnet/tools/ for .NET CLI tools
- Check /opt/ for large third-party apps

TO DIFF FROM STOCK UBUNTU:
-------------------------
1. Download stock manifest:
   wget https://cloud-images.ubuntu.com/$(lsb_release -cs)/current/$(lsb_release -cs)-server-cloudimg-amd64.manifest

2. Extract package names:
   grep -oP '^[a-z0-9\-\+\.]+' manifest | sort -u > stock.txt

3. Compare with installed:
   dpkg -l | grep '^ii' | awk '{print $2}' | sort -u > current.txt
   comm -23 current.txt stock.txt > custom-packages.txt

EOF

echo ""
echo "=== INVENTORY COMPLETE ==="
echo "Location: $OUTPUT_DIR"
echo ""
echo "Quick Summary:"
echo "- APT packages: $(dpkg -l | grep '^ii' | wc -l)"
echo "- Snaps: $(snap list 2>/dev/null | tail -n +2 | wc -l)"
echo "- Flatpaks: $(flatpak list 2>/dev/null | wc -l)"
echo "- NPM globals: $(npm list -g --depth=0 2>/dev/null | tail -n +2 | wc -l)"
echo "- Cargo packages: $(cargo install --list 2>/dev/null | grep -c '^[a-z]' || echo 0)"
echo "- Pipx packages: $(pipx list --short 2>/dev/null | wc -l)"
echo "- /usr/local/bin: $(ls /usr/local/bin/ 2>/dev/null | wc -l)"
echo "- ~/.local/bin: $(ls ~/.local/bin/ 2>/dev/null | wc -l)"
echo ""
echo "See $OUTPUT_DIR/INVENTORY-SUMMARY.txt for details"
```

---

## 7. Identifying Specific Tool Categories

### 7.1 Development Tools

```bash
# IDEs and Editors
echo "=== IDEs/Editors ==="
which code vim nvim emacs nano pycharm idea rider webstorm 2>/dev/null

# Version Control
echo "=== Version Control ==="
which git svn hg fossil 2>/dev/null
git --version 2>/dev/null

# Build Tools
echo "=== Build Tools ==="
which make cmake ninja meson gradle mvn ant cargo 2>/dev/null

# Compilers
echo "=== Compilers ==="
which gcc g++ clang rustc go javac python python3 node dotnet 2>/dev/null
```

### 7.2 CLI Productivity Tools

```bash
# Search tools
echo "=== Search ==="
which fzf rg ag fd find 2>/dev/null

# File managers
echo "=== File Managers ==="
which ranger nnn lf mc vifm 2>/dev/null

# Terminal multiplexers
echo "=== Terminal Multiplexers ==="
which tmux screen byobu zellij 2>/dev/null

# Text processing
echo "=== Text Processing ==="
which jq yq awk sed perl 2>/dev/null

# Modern alternatives
echo "=== Modern CLI Tools ==="
which bat eza lsd delta dust duf procs sd choose 2>/dev/null
```

### 7.3 Cloud/DevOps Tools

```bash
# Container tools
echo "=== Containers ==="
which docker podman nerdctl containerd 2>/dev/null
docker --version 2>/dev/null

# Kubernetes
echo "=== Kubernetes ==="
which kubectl helm kustomize kubectx kubens k9s minikube kind k3s 2>/dev/null

# Cloud CLIs
echo "=== Cloud CLIs ==="
which aws az gcloud oci doctl vultr scw 2>/dev/null

# Infrastructure as Code
echo "=== IaC Tools ==="
which terraform pulumi ansible vagrant packer 2>/dev/null
```

---

## 8. Quick Reference Commands

| Task | Command |
|------|---------|
| All APT packages | `dpkg -l` |
| All Snaps | `snap list` |
| All Flatpaks | `flatpak list` |
| NPM globals | `npm list -g --depth=0` |
| Yarn globals | `yarn global list` |
| Bun packages | `bun pm ls` |
| Cargo packages | `cargo install --list` |
| .NET global tools | `dotnet tool list -g` |
| Pipx packages | `pipx list --short` |
| User pip packages | `pip list --user` |
| /usr/local/bin | `ls -la /usr/local/bin/` |
| ~/.local/bin | `ls -la ~/.local/bin/` |
| ~/.cargo/bin | `ls -la ~/.cargo/bin/` |
| ~/.dotnet/tools | `ls -la ~/.dotnet/tools/` |
| ~/.bun/bin | `ls -la ~/.bun/bin/` |
| PATH executables | `find $(echo $PATH \| tr ':' ' ') -maxdepth 1 -type f -executable` |
| System services | `systemctl list-unit-files --state=enabled` |

---

## 9. References

- [Ubuntu Package Manifests](https://cloud-images.ubuntu.com/) - Official package lists for each Ubuntu release
- [diffoscope Documentation](https://diffoscope.org/) - Deep comparison tool
- [NPM Global Packages](https://docs.npmjs.com/cli/v8/commands/npm-list) - NPM listing docs
- [Bun Package Manager](https://bun.com/docs/pm/cli/pm) - Bun pm commands
- [Cargo Install](https://doc.rust-lang.org/cargo/commands/cargo-install.html) - Rust package management
- [.NET Global Tools](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) - .NET CLI tool management
- [Pipx Documentation](https://pipx.pypa.io/stable/) - Python application isolation
- [Snap Documentation](https://snapcraft.io/docs/command-reference)
- [Flatpak Documentation](https://docs.flatpak.org/en/latest/flatpak-command-reference.html)

---

**End of Report**

*Use the system-inventory.sh script to generate a complete snapshot of all installed tools on this Ubuntu system.*
