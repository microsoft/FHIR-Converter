# Template Management 
Template Management is a tool to manage template files which is a connection between local and external storage. It organizes template files as an image and allows pull and push operations to manage different versions.

Template image is a layer based data structure similar with docker image and using overlayfs concept to organize templates.

For our template image, we use two layers to organize one template collection: base layer and user layer (The user layer could be extended to multi-layers in the future if necessary). Base layer packs official published template collection. User layer packs all modified templates by users. Each layer will be packed into "*.tar.gz" file before pushing to ACR.
# Using Template Management CLI

The command-line tool can be used to pull and push a template collection through remote registry (Now we only support Azure Container Registry). Before pull & push operations, oras login is required.

## Oras Login
The oras.exe is packed in our repo, users can directly use it for login as follows.

```
>.\oras.exe login <registry> -u <username> -p <password>
```
Users can also follows the [Oras](https://github.com/deislabs/oras) instructure for oras download and login.

## Pull 
In pull operation, the command is 

```
pull <ImageReference> [-o | --OutputTemplateFolder] [ -f | --ForceOverride]
```

| Value | index |Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| ImageReference |0| Required | | Image reference: \<registry>\/\<imageName>\:\<imageTag> |

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -o | OutputTemplateFolder | Optional | "." | Output template folder. |
| -f | ForceOverride | Optional | false | Force to override the output folder. |

Example usage to pull an image of templates in a folder:

```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe pull <registry>/<imageName>:<imageTag> -i myOutputFolder
```

When pull a collection of templates, all template files will be pulled into the output folder. A hidden folder "/.image/" will be create in this output folder as well, which stores meta and layer information. Users shouldn't modify this hidden folder which may lead to unexpected result for the push operation.

## Push
In push operation, the command is 

```
push <ImageReference> [ -i | --InputTemplateFolder] [ -n | --NewBaseLayer]
```
| Value | index |Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| ImageReference |0| Required | | Image reference: \<registry>\/\<imageName>\:\<imageTag> |

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -i | InputTemplateFolder | Optional | "." | Input template folder. |
| -n | BuildNewBaseLayer | Optional | false | Ignore previous base layer and build new layer. |

Example usage to push a collection of templates to ACR image from a folder:

```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe push <registry>/<imageName>:<imageTag> -i myInputFolder
```

When push templates, all files in the input folder except files in the hidden image folder ("/.image/") will be packed. The default behavior is packing modified templates on base layer using overlay concept (The base layer is stores in hidden folder "/.image/"). If customers using -n as parameter, all templates in input folder will be packed as new layer and be pushed as the base layer of an image.