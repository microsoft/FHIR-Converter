# Template Management 
Template Management is a tool to manage template files between local and external storage. It organizes template files as a image and allows pull and push operations to manage versions.

Template image is a layer based data structure similar with docker image and using overlayfs concept to organize templates.

For our template image, we use two layers to organize one version of template set: base layer and user layer (The user layer could be extended to multi-layers in the future if necessary). Base layer packs the  published template set. User layer packs all modified templates by users. Each layer will be packed into "*.tar.gz" file before pushing to ACR.
# Using Template Management CLI

The command-line tool can be used to pull and push a template collection through remote registry. (Now we only support azure container registry). Before pull & push operation, oras login is requied.

## Oras Login
The oras.exe is packed in our tool, using oras command line to login acr is required.

```
>.\oras.exe login [registry] -u [username] -p [password]
```

## Pull 
| Value | index |Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| ImageReference |0| Required | | Image reference: \<registry>\/\<imageName>\:\<imageTag> |

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -o | OutputTemplateFolder | Optional | "." | Output template folder. |

Example usage to pull an image of templates in a folder:

```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe pull [registry]/[imageName]:[imageTag] -i [myOutputFolder]
```

When pull a collection of templates, all template files will be pulled into the output folder. A hidden folder "/.image/" will be create in this output folder as well, which stores meta and layer information. Users shouldn't modify this hidden folder which may lead to unexpected result for the push operation.

## Push

| Value | index |Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| ImageReference |0| Required | | Image reference: \<registry>\/\<imageName>\:\<imageTag> |

| Option | Name | Optionality | Default | Description |
| ----- | ----- | ----- |----- |----- |
| -i | InputTemplateFolder | Optional | "." | Input template folder. |
| -n | BuildNewBaseLayer | Optional | false | Ignore previous base layer and build new layer. |

Example usage to push a collection of templates to ACR image from a folder:

```
>.\Microsoft.Health.Fhir.Liquid.Converter.Tool.exe push [registry]/[imageName]:[imageTag] -i [myInputFolder]
```

When push templates, all files in the input folder except files in the hidden image folder ("/.image/") will be packed.the default behavior is packing modified templates on base layer using overlay concept. (The base layer is stores in hidden folder "/.image/"). If customers using -n as parameter, all templates in input folder will be packed as new layer and be pushed as the base layer of an image.