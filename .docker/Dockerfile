# Development Dockerfile with more tools and hot reload capabilities
FROM mcr.microsoft.com/dotnet/sdk:8.0 as development

# Install additional development tools
RUN apt-get update && apt-get install -y \
    make \
    curl \
    wget \
    git \
    vim \
    nano \
    htop \
    tree \
    jq \
    && rm -rf /var/lib/apt/lists/*

# Set the working directory
WORKDIR /app

# Copy project files for dependency caching
COPY *.csproj ./
RUN dotnet restore

# Install dotnet tools
RUN dotnet tool install --global dotnet-ef
RUN dotnet tool install --global dotnet-watch

# Make sure dotnet tools are in PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy the rest of the source code
COPY . ./

# Default command for development
CMD ["dotnet", "watch", "run"]
