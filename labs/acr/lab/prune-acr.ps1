
$acrName = <acr-name>
$acrMaxImageTags=5

$repositories = az acr repository list --name $acrName --output tsv
foreach ($repository in $repositories) {
    echo "Pruning repository $repository" | timestamp
    $tags = az acr repository show-tags --name $acrName --repository $repository --output tsv --orderby time_desc

    if ($tags.length -gt $acrMaxImageTags) {
        echo "Repository tag count: $($tags.length); max tags: $acrMaxImageTags; pruning excess"
        for ($i=$acrMaxImageTags; $i -lt $tags.length; $i++) {
            $imageName = "$($repository):$($tags[$i])"
            echo "Deleting: $imageName" | timestamp
            az acr repository delete --name $acrName --image $imageName --yes --only-show-errors
        }
    }
}

# credits:
# https://github.com/andrew-kelleher/azurecontainerregistry-cleanup/blob/master/acr-cleanup.ps1