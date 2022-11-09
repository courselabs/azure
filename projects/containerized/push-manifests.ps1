$images=$('courselabs/todo-list-web:2211', 'courselabs/todo-list-save-handler:2211')

foreach ($image in $images)
{    
    docker manifest create --amend $image `
      "$($image)-linux-arm64" `
      "$($image)-linux-amd64"
    
    docker manifest push $image
}
