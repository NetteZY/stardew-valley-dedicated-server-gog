# JunimoServer

<!-- Project -->

![GitHub Tag](https://img.shields.io/github/v/tag/stardew-valley-dedicated-server/server?label=Latest%20Release&style=flat-square&colorA=18181B) ![Static Badge](https://img.shields.io/badge/Stardew%20Valley-v1.6.15-34D058?style=flat-square&colorA=18181B) [![CodeQL](https://img.shields.io/github/actions/workflow/status/stardew-valley-dedicated-server/server/codeql.yml?branch=master&label=CodeQL&style=flat-square&colorA=18181B)](https://github.com/stardew-valley-dedicated-server/server/actions/workflows/codeql.yml) [![E2E Tests](https://img.shields.io/endpoint?url=https%3A%2F%2Fpub-8b02482b459740d1b403ddbc47d0b817.r2.dev%2Fe2e%2Fmaster%2Fbadge.json&style=flat-square&colorA=18181B)](https://pub-8b02482b459740d1b403ddbc47d0b817.r2.dev/e2e/master/latest/index.html) [![Discord](https://img.shields.io/discord/947923329057185842?label=Discord&logo=discord&color=34D058&style=flat-square&colorA=18181B)](https://discord.gg/w23GVXdSF7)

**JunimoServer** makes [Stardew Valley](https://www.stardewvalley.net/) multiplayer hosting simple and flexible. Host your farm anytime, anywhere — on your local machine, a VPS, or a dedicated server.

This open-source project enables 24/7 multiplayer farms without needing to keep the game running on your machine. Players can connect at any time without requiring you to be online. With customizable settings, automated backups, and support for larger farms, JunimoServer makes multiplayer management easier than ever.

### Table of Contents

<!-- REGENERATE TOC: npx markdown-toc -i README.md -->

<!-- toc -->

- [Features](#features)
- [Quick start](#quick-start)
    - [Prerequisites](#prerequisites)
    - [Getting started](#getting-started)
    - [Updating to a new version](#updating-to-a-new-version)
    - [Using preview releases](#using-preview-releases)
- [Documentation](#documentation)
- [Support](#support)

<!-- tocstop -->

## Features

JunimoServer gives you everything you need to host Stardew Valley:

- **Always-On Hosting**: Keep your farm running 24/7 without needing to leave the game open.
- **Easy Management**: Control your server through a simple, web-based interface with admin capabilities.
- **Persistent Progress**: Protect your crops and ensure your farm continues to thrive, even when no one’s online.
- **Automatic Backups**: Regularly save your farm so you can easily restore it if something goes wrong.
- **Fully Customizable**: Change game modes, tweak settings, and optimize performance to fit your needs.
- **Mod-Friendly**: Supports SMAPI mods to enhance your Stardew Valley experience with customizations and extra content.

## Quick start

### Prerequisites

- **Docker**: Install [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/Mac) or [Docker Engine](https://docs.docker.com/engine/install/) (Linux).
- **Stardew Valley GOG Game Files**: You must own Stardew Valley on GOG and download the **Offline Backup Installer for Linux** (a file named `stardew_valley_*.sh`).

---

### Hosting a Server (Using the Pre-built Image)

Since GOG game files are copyrighted, they are **not** bundled inside our Docker image. However, you do **not** need to build the Docker image or compile the C# mod yourself. You can pull the pre-built server image and mount your local game files into the container.

#### 1. Extract your GOG Game Files
Download the Linux offline installer (`stardew_valley_*.sh`) from GOG and extract the raw game files into a directory named `game-files`:

*   **Windows**:
    1. Open the `.sh` installer file using [7-Zip](https://7-zip.org/).
    2. Inside the archive, navigate to `data/noarch/game/`.
    3. Extract all files inside that folder into a folder named `game-files` on your machine.
*   **macOS / Linux**:
    Run the following command in your terminal to extract the game files:
    ```bash
    unzip -q stardew_valley_*.sh "data/noarch/game/*" -d game-files-tmp
    mkdir -p game-files
    cp -r game-files-tmp/data/noarch/game/. game-files/
    rm -rf game-files-tmp
    ```

#### 2. Create the Configuration Files
In the same directory where your `game-files` folder is located, create a file named `docker-compose.yml` with the following contents:

```yaml
services:
  server:
    # Replace with the repository/registry image path of your published fork
    image: ghcr.io/YOUR_GITHUB_USERNAME/stardew-valley-dedicated-server-gog:latest
    container_name: sdvd-server
    stdin_open: true
    tty: true
    ports:
      - "5800:5800"       # VNC Web GUI (Access in browser at http://localhost:5800)
      - "8089:8080"       # REST API Port (Mapped to 8089 to avoid conflicts)
      - "24642:24642/udp" # Game server port (UDP)
    cap_add:
      - SYS_TIME          # Required to synchronize time for secure handshakes
    volumes:
      - ./game-files:/data/game
      - saves:/config/xdg/config/StardewValley
      - ./settings:/data/settings
    environment:
      VNC_PASSWORD: "ChooseYourVncPassword"       # Password to access VNC web UI (can be empty if insecure)
      ALLOW_INSECURE_SETUP: "true"                # Bypasses warning check if API_KEY/VNC_PASSWORD are empty
      SERVER_TPS: "60"
      SERVER_FPS: "0"
      SETTINGS_PATH: "/data/settings/server-settings.json"
      API_ENABLED: "true"
      API_PORT: "8080"
      API_KEY: "ChooseYourApiKey"                 # API key for the REST API interface
    restart: unless-stopped

volumes:
  saves:
```

#### 3. Start the Server
Run the following command in the directory containing your `docker-compose.yml` and `game-files` folder:
```bash
docker compose up -d
```

#### 4. Connect to Your Server
Because Steam features are disabled in this GOG fork:
1. Launch Stardew Valley.
2. Go to **Co-op** -> **Join LAN Game**.
3. Enter your server's IP address and port `24642` (e.g. `192.168.1.100:24642`).

---

### Developing & Building from Source (Local Compilation)

If you want to modify the C# mod code or build the Docker image yourself:

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/YOUR_GITHUB_USERNAME/stardew-valley-dedicated-server.git
   cd stardew-valley-dedicated-server/stardew-server
   ```

2. **Place the GOG Installer**:
   Place the GOG Linux offline installer `stardew_valley_*.sh` in the root of the `stardew-server` directory.

3. **Extract Game Files**:
   Run the Makefile target to extract GOG files to `./game-files`:
   ```bash
   make extract-gog INSTALLER=stardew_valley_*.sh
   ```

4. **Build the Server Image**:
   This compiles the C# mod against your extracted game files and bundles the mod inside the Docker image:
   ```bash
   make build
   ```

5. **Start Your Local Container**:
   ```bash
   make up
   ```


## Documentation

Explore the [full documentation](https://stardew-valley-dedicated-server.github.io/server/) to get started. Here's what you'll find:

- **[Getting Started](https://stardew-valley-dedicated-server.github.io/server/getting-started/introduction):** Step-by-step instructions on setting up and managing your server.
- **[Server Guide](https://stardew-valley-dedicated-server.github.io/server/guide/using-the-server):** Learn how to use and manage your server.
- **[Community](https://stardew-valley-dedicated-server.github.io/server/community/getting-help):** Find out how to get involved and get help.

## Support

JunimoServer is free and open-source, maintained in spare time. If it keeps your farm running and you'd like to give something back, donations help cover **server/hosting costs** and **development time** — entirely optional, always appreciated. 🌱

- **[GitHub Sponsors](https://github.com/sponsors/JulianVallee)** — monthly or one-time, 100% goes to development.
- **[Ko-fi](https://ko-fi.com/junimoserver)** — buy the project a coffee (one-time, PayPal or card).

Not in a position to donate? Starring the repo, reporting bugs, improving the docs, or helping others on [Discord](https://discord.gg/w23GVXdSF7) helps just as much.
