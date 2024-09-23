# NTK 2024 - demo repository

Demos for the [NTK 2024](https://www.ntk.si) session Container Options in Azure (
more [here](https://www.ntk.si/urnik/predavanje/uporaba_kontajnerjev_v_platformi/51) ).

To explore different approaches for launching and managing containers in the Azure platform (Azure
Container Apps, Azure Kubernetes Services, Azure Container Instances, Azure Web App for Containers, etc.), compare them
and show their advantages and disadvantages, ways of integration into other information systems and use for easy
installation, maintenance and management according to the number of customers and service requirements, I focus on:

1. **Demo applications** - simple web applications that we will deploy to the Azure platform using different container
   options.
2. **Demo APIs** - simple APIs that we will deploy to the Azure platform using different container options. Majority of
   the api will be privately
   available only to the web application (with few public options access availability).
3. **Demo databases** - simple databases that we will deploy to the Azure platform using different container options.
4. **Scripts** - scripts for easy deployment and management of the above.

## Prerequisites

1. An active [Azure](https://www.azure.com) subscription - [MSDN](https://my.visualstudio.com) or trial
   or [Azure Pass](https://microsoftazurepass.com) is fine - you can also do all of the work
   in [Azure Shell](https://shell.azure.com) (all tools installed) and by
   using [Github Codespaces](https://docs.github.com/en/codespaces/developing-in-codespaces/creating-a-codespace)
2. [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/) installed to work with Azure
3. [GitHub](https://github.com/) account (sign-in or join [here](https://github.com/join)) - how to authenticate with
   GitHub
   available [here](https://docs.github.com/en/get-started/quickstart/set-up-git#authenticating-with-github-from-git)
4. [RECOMMENDATION] [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.2)
   installed - we do recommend an editor like [Visual Studio Code](https://code.visualstudio.com) to be able to write
   scripts, YAML pipelines and connect to repos to submit changes.
5. [OPTIONAL] GitHub CLI installed to work with GitHub - [how to install](https://cli.github.com/manual/installation)
6. [OPTIONAL] [Github GUI App](https://desktop.github.com/) for managing changes and work
   on [forked](https://docs.github.com/en/get-started/quickstart/fork-a-repo) repo
7. [OPTIONAL] [Windows Terminal](https://learn.microsoft.com/en-us/windows/terminal/install)

If you will be working on your local machines, you will need to have:

1. [Powershell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.2)
   installed
2. git installed - instructions step by step [here](https://docs.github.com/en/get-started/quickstart/set-up-git)
3. [.NET](https://dot.net) installed to run the application if you want to run it
4. an editor (besides notepad) to see and work with code, yaml, scripts and more (for
   example [Visual Studio Code](https://code.visualstudio.com))

## Scripts

Scripts are available in [scripts folder](./scripts). The scripts are written
in [PowerShell](https://docs.microsoft.com/en-us/powershell/scripting/overview?view=powershell-7.2).

1. [Add-DirToSystemEnv.ps1](./scripts/Add-DirToSystemEnv.ps1) - adds a directory to the system environment variable
   PATH
2. [Install-AZCLI.ps1](./scripts/Install-AZCLI.ps1) - installs Azure CLI
3. [Install-Bicep.ps1](./scripts/Install-Bicep.ps1) - installs Bicep language

## Demo applications

*NTK24.Web* is a simple web application which enables users to save their favorite links and then be able to access them.

![NTK24 web app](https://webeudatastorage.blob.core.windows.net/web/ntk24-web.png)

*NTK24.Api* is a simple API that will be deployed to the Azure platform using different container options.

![NTK24 api](https://webeudatastorage.blob.core.windows.net/web/ntk24-api.png)

*NTK24.Init* is a simple API that will be deployed to the Azure platform using different container options.

![NTK24 init](https://webeudatastorage.blob.core.windows.net/web/ntk24-init.png)

## Demo databases

Database is simple and can be initialized with a script [here](scripts/database-init.sql).

![NTK database](https://webeudatastorage.blob.core.windows.net/web/ntk24-db.png)

## Scripts

Scripts to help deploying to the cloud and working with demos are available in the `scripts` directory. They are written
in PowerShell and use Bicep do deploy infrastructure as code and to help with applications.

[Docker files](./containers) are available to build and run the application in containers. You can also leverage helper
script [Compile-Containers.ps1](./scripts/Compile-Containers.ps1) to build containers
using [Azure Container Registry task builders](https://learn.microsoft.com/en-us/azure/container-registry/container-registry-tutorial-build-task).

# Links and additional information

1. [NTK 2024](https://www.ntk.si)
2. [Azure Portal](https://portal.azure.com/)
3. [Azure Container Apps](https://azure.microsoft.com/en-us/services/container-apps/)
4. [Azure Kubernetes Services](https://azure.microsoft.com/en-us/services/kubernetes-service/)
5. [Azure Container Instances](https://azure.microsoft.com/en-us/services/container-instances/)
6. [Azure Web App for Containers](https://azure.microsoft.com/en-us/services/app-service/containers/)