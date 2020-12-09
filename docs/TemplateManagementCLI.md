Template Management CLI is a tool to manage template files for FHIR Converter DotLiquid engine. This CLI tool manages all template files in [OCI image](https://github.com/opencontainers/image-spec) format and allows pushing and pulling templates with [Azure Container Registry](https://azure.microsoft.com/en-us/services/container-registry/).

Template image is a layer based structure similar to docker image and uses [overlayfs](https://www.kernel.org/doc/html/latest/filesystems/overlayfs.html?highlight=overlayfs) concept to organize templates.

For custom templates, we use two layers image structure to organize template collection: base layer and user layer (The user layer could be extended to multi-layers in the future if necessary). Base layer packs official published templates and user layer packs all modified templates from users. Each layer will be compressed into "*.tar.gz" file before pushing to ACR.
# Using Template Management CLI

The command-line tool can be used to pull and push a template collection through remote registry (Now we only support Azure Container Registry). 

## Prerequisites
* Azure container registry - Create a container registry in your Azure subscription. For example, use the [Azure portal](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-portal) or the [Azure CLI](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-azure-cli).

* Azure Active Directory service principal (optional) - If using service principal's identity for authentication, you need to create a [service principal](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-auth-service-principal) to access your registry. Ensure that the service principal is assigned a role such as AcrPush so that it has permissions to push and pull artifacts.

* Azure CLI (optional) - To use an individual identity, you need a local installation of the Azure CLI. Version 2.0.71 or later is recommended. Run `az --version` to find the version. If you need to install or upgrade, see [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli).

* Docker (optional) - To use an individual identity, you must also have Docker installed locally, to authenticate with the registry. Docker provides packages that easily configure Docker on any [macOS](https://docs.docker.com/docker-for-mac/), [Windows](https://docs.docker.com/docker-for-windows/), or [Linux](https://docs.docker.com/engine/install/) system.
## Authentication

Before pull & push operations, azure authentication is required for private registries. Customers can directly use individual login with Azure AD through [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli) or use identity (individual identity or Azure AD [service principal identity](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-auth-service-principal)) to sign in the registry. 

### Login Using Azure CLI

After signing in to the Azure CLI with your identity, use the Azure CLI command `az acr login` to access the registry.
```
> az acr login --name <registry>
```

### Login Using Identity (individual or service principal indentity)

* Docker login

```
> docker login <registry> -u <username> -p <password>
```
* Oras Login

The [oras](https://github.com/deislabs/oras) tool oras.exe is packed in our repo, users can directly use it for login as follows.

```
>.\oras.exe login <registry> -u <username> -p <password>
```

## Push
To push a template collection, the command is: 

```
push <ImageReference> InputTemplateFolder [ -n | --NewBaseLayer]
```
| Value | index |Optionality |  Description |
| ----- | ----- | ----- |----- |
| ImageReference |0| Required |  Image reference: \<Registry>\/\<Name> \[:\<Tag>]  (Image name should be lowcase.)|
|InputTemplateFolder | 1 |Required |Input template folder. |

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -n | BuildNewBaseLayer | Optional | false | Ignore previous base layer and build new layer. |

Example usage to push a collection of templates to ACR image from a folder:

```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe push testacr.azurecr.io/templatetest:default myInputFolder
```
When pushing templates, all files except files in hidden image folder ("./.image/") will be packed as new template image. If the folder is unpacked from a previous template image, our tool will pack all user modified files into the user layer and then push all layers to ACR (The base layer is stores in hidden folder "./.image/"). If customers using -n as parameter, all templates will be packed together and be pushed as one layer to ACR.

After successfully pushing an image, relevant information including layers' digests and image digest will output to users. Here is an output example, users should remember the image digest which exactly index an image:

```
Uploading 4085e9f97630 layer2.tar.gz
Uploading 4157f847ecb1 layer1.tar.gz
Pushed testacr.azurecr.io/templatetest:default
Digest: sha256:412ea84f1bb1a9d98345efb7b427ba89616ec29ac332d543eff9a2161ca12a58
```

## Pull 
For pull operation, the command is 

```
pull <ImageReference> <OutputTemplateFolder> [ -f | --ForceOverride]
```

| Value | index |Optionality |  Description |
| ----- | ----- | ----- |----- |
| ImageReference |0| Required |  Image reference: \<Registry>\/\<Name>\[ @\<Digest> \| :\<Tag> \] |
|OutputTemplateFolder | 1 |Required | Output template folder. |

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -f | ForceOverride | Optional | false | Force to override the output folder. |

Example usage to pull an image of templates in a folder:

```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe pull testacr.azurecr.io/templatetest@sha256:412ea84f1bb1a9d98345efb7b427ba89616ec29ac332d543eff9a2161ca12a58 myOutputFolder
```

After a collection of templates is pulled, a hidden folder ".image/" which contains information of metadata and layers is also created in the output folder. Users shouldn't modify this hidden folder which may lead to unexpected result.

Image tags are mutable and could be overwritten unintentionally. We recommend you write down the image digest and use the immutable image digest as the template reference. Users should remember the image digest when pushing or find digest from ACR, since it won't be searched by our tool for now.   

