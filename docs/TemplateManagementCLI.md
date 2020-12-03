Template Management is a tool to manage template files which is a connection between local and external storage. It organizes template files in image and allows pull and push operations. 

Template image is a layer based data structure similar to docker image and uses overlayfs concept to organize templates.

For user's templates, we use two layers structure to organize template collection: base layer and user layer (The user layer could be extended to multi-layers in the future if necessary). Base layer packs official published template collection and user layer packs all modified templates from users. Each layer will be packed into "*.tar.gz" file before pushing to ACR.
# Using Template Management CLI

The command-line tool can be used to pull and push a template collection through remote registry (Now we only support Azure Container Registry). Before pull & push operations, oras login is required.

## Oras Login
The oras.exe is packed in our repo, users can directly use it for login as follows.

```
>.\oras.exe login <registry> -u <username> -p <password>
```
Users can also follow the [Oras](https://github.com/deislabs/oras) instruction for oras download and login.

## Pull 
In pull operation, the command is 

```
pull <ImageReference> <OutputTemplateFolder> [ -f | --ForceOverride] [ -e | ErrorJsonFile]
```

| Value | index |Optionality |  Description |
| ----- | ----- | ----- |----- |----- |
| ImageReference |0| Required |  Image reference: \<Registry>\/\<Name>\[ :\<Tag> \| @\<Digest>] |
|OutputTemplateFolder | 1 |Required | Output template folder. |

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -f | ForceOverride | Optional | false | Force to override the output folder. |
| -e | ErrorJsonFile | Optional | null | File path for output error message. |

Example usage to pull an image of templates in a folder:

```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe pull <Registry>/<Name>@<Digest> myOutputFolder
```

When pull a collection of templates, all template files will be pulled into the output folder. A hidden folder "/.image/" will be create in this output folder as well, which stores meta and layer information. Users shouldn't modify this hidden folder which may lead to unexpected result for the push operation.

> [Note!]:
Image labeled by tag could be override easily, using digest is more safe when pulling an image. Users should remember the image digest when pushing or find digest from ACR, since it won't be searched by our tool for now.   

## Push
In push operation, the command is: 

```
push <ImageReference> InputTemplateFolder [ -n | --NewBaseLayer] [ -e | ErrorJsonFile]
```
| Value | index |Optionality |  Description |
| ----- | ----- | ----- |----- |----- |
| ImageReference |0| Required |  Image reference: \<registry>\/\<imageName> \[:\<imageTag>] |
|InputTemplateFolder | 1 |Required |Input template folder. |

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -n | BuildNewBaseLayer | Optional | false | Ignore previous base layer and build new layer. |
| -e | ErrorJsonFile | Optional | null | File path for output error message. |

Example usage to push a collection of templates to ACR image from a folder:

```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe push <Registry>/<Name>:<Tag> myInputFolder
```

When push templates, all files in the input folder except files in the hidden image folder ("/.image/") will be packed. The default behavior is packing modified templates on base layer using overlay concept (The base layer is stores in hidden folder "/.image/"). If customers using -n as parameter, all templates in input folder will be packed as new layer and be pushed as the base layer of an image.