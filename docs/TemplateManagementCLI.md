The $convert-data operation in the FHIR Server for Azure takes templateCollectionReference parameter, which refers to an [OCI image ](https://github.com/opencontainers/image-spec) on [Azure Container Registry (ACR)](https://azure.microsoft.com/en-us/services/container-registry/). It is the image containing Liquid templates to use for conversion.

The Template Management CLI tool is mean to pull, push, and manage the templates on the ACR.

Template OCI image is a layer based structure similar to docker image and uses [overlayfs](https://www.kernel.org/doc/html/latest/filesystems/overlayfs.html?highlight=overlayfs) concept to organize templates. For custom templates, we use two layers image structure to organize template collection: base layer and user layer (The user layer could be extended to multi-layers in the future if necessary). Base layer packs Microsoft published templates and user layer packs all modified templates from users. Each layer will be compressed into "*.tar.gz" file before pushing to ACR.
# Using Template Management CLI

The command-line tool can be used to pull and push a template collection from/to Azure Container Registry. Currently, we support Windows and MacOS for the CLI tool.

## Prerequisites
* Azure container registry - Create a container registry in your Azure subscription if you do not have one. This is the registry where you want to keep your Liquid templates. You can use the [Azure portal](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-portal) or the [Azure CLI](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-get-started-azure-cli).


## Authentication

Before pull & push operations, azure authentication is required for private registries. Customers can directly use individual login with Azure AD through [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/authenticate-azure-cli) or use identity (individual identity or Azure AD [service principal identity](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-auth-service-principal)) to sign in the registry. 

### Login Using Azure CLI

To use individual login with Azure AD, you need a local installation of the Azure CLI. The latest version is recommended. Run `az --version` to find the version. If you need to install or upgrade, see [Install Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli).

You should first sign in to the Azure CLI with your identity, and then use the Azure CLI command `az acr login` to access the registry. Make sure you have permissions on the registry in order to pull/push.
```
> az login
> az acr login --name <acrName>
```

### Login Using Identity (individual or service principal indentity)


* Docker login

To use docker login, you should install docker first.
```
> docker login <acrName.azurecr.io> -u <username> -p <password>
```
* Oras Login

The [Oras](https://github.com/deislabs/oras) executable tools (oras.exe for windows and oras-osx for mac) are packed in our repo, users can directly use it for login as follows.

```
> <orasExeTool> login <acrName.azurecr.io> -u <username> -p <password>
```
>Note: macOS users need to add execute permission for oras-osx file before using. 
>```
>chmod +x oras-osx
>```

If using service principal's identity for authentication, you need to create a [service principal](https://docs.microsoft.com/en-us/azure/container-registry/container-registry-auth-service-principal) to access your registry. Ensure that the service principal is assigned a role such as AcrPush so that it has permissions to push and pull artifacts.
## Push
To push a template collection, the command is:

```
push <ImageReference> InputTemplateFolder [ -n | --NewBaseLayer]
```
| Value | index |Optionality |  Description |
| ----- | ----- | ----- |----- |
| ImageReference |0| Required |  Image reference: \<registry>\/\<name>:\<tag>  (Here is the [reference](https://docs.docker.com/engine/reference/commandline/tag/#extended-description) for the valid format.)|
|InputTemplateFolder | 1 |Required |Input template folder. |

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -n | BuildNewBaseLayer | Optional | false | Ignore previous base layer and build new layer. |

Example command to push a collection of templates to ACR image from a folder:

```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe push testacr.azurecr.io/templatetest:default myInputFolder
```
When pushing templates, all files except files in hidden image folder ("./.image/") will be packed as new template image. If the folder is unpacked from a previous template image, our tool will pack all user modified files into the user layer and then push all layers to ACR (The base layer is in hidden folder "./.image/"). If customers use -n as parameter, all templates will be packed together and be pushed as one layer to ACR.

>[Note!]: As for template OCI image, entry templates should be present directly in the root folder.

After successfully pushing an image, relevant information including layers' digests and image digest will output to users. Here is an output example, users should remember the image digest which exactly indexes an image:

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
| ImageReference |0| Required |  Image reference: \<registry>\/\<name>@\<digest> or  \<registry>\/\<name>\:\<tag> |
|OutputTemplateFolder | 1 |Required | Output template folder. |

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -f | ForceOverride | Optional | false | Force to override the output folder. |

Example usage of pulling an image of templates in a folder:

```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe pull testacr.azurecr.io/templatetest@sha256:412ea84f1bb1a9d98345efb7b427ba89616ec29ac332d543eff9a2161ca12a58 myOutputFolder
```

After a collection of templates is pulled, a hidden folder ".image/" which contains information of metadata and layers is also created in the output folder. Users shouldn't modify this hidden folder which may lead to unexpected results.

Image tags are mutable and could be overwritten unintentionally. We recommend you write down the image digest and use the immutable image digest as the template reference. Users should remember the image digest when pushing or find digest from ACR, since it won't be searched by our tool for now.   

