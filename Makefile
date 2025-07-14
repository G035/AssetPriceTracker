CONFIGURATION=Release
WINDOWS_RID=win-x64
LINUX_RID=linux-x64
OUTPUT_DIR=publish

build:
	dotnet build -c $(CONFIGURATION)

publish-windows:
	dotnet publish -c $(CONFIGURATION) -r $(WINDOWS_RID) --self-contained true -o $(OUTPUT_DIR)/windows

publish-linux:
	dotnet publish -c $(CONFIGURATION) -r $(LINUX_RID) --self-contained true -o $(OUTPUT_DIR)/linux

publish-all: publish-windows publish-linux

clean:
	dotnet clean
	rm -rf $(OUTPUT_DIR)

help:
	@echo "Available commands:"
	@echo "  make build             - Build the project"
	@echo "  make clean             - Clean build artifacts"
	@echo "  make publish-windows   - Self-contained build for Windows"
	@echo "  make publish-linux     - Self-contained build for Linux"
	@echo "  make publish-all       - Build for both platforms"
