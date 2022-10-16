
$acrName='labsacres'
$acrMaxImageTags=1

$repositories = az acr repository list --name $acrName --output tsv
foreach ($repository in $repositories) {
    echo "Pruning repository $repository"
    $tags = az acr repository show-tags --name $acrName --repository $repository --orderby time_desc | ConvertFrom-Json

    if ($tags.count -gt $acrMaxImageTags) {
        echo "Repository tag count: $($tags.count); max tags: $acrMaxImageTags; pruning excess"
        for ($i=$acrMaxImageTags; $i -lt $tags.count; $i++) {
            $imageName = "$($repository):$($tags[$i])"
            echo "Deleting: $imageName"
            az acr repository delete --name $acrName --image $imageName --yes --only-show-errors
        }
    }
}

# credits:
# https://github.com/andrew-kelleher/azurecontainerregistry-cleanup/blob/master/acr-cleanup.ps1