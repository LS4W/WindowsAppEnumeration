# Compiling with MONO

## Ubuntu 20.04
```bash
# Install mono and nuget
sudo apt install -y gnupg ca-certificates
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb https://download.mono-project.com/repo/ubuntu stable-focal main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
sudo apt update
sudo apt install -y mono-complete nuget

# Compile
nuget restore LS4W.sln
msbuild /p:Configuration=Release LS4W.sln
```