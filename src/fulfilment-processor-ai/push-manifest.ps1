$images=$('courselabs/fulfilment-processor:appinsights-1.0', 'courselabs/fulfilment-processor:appinsights-1.2')

foreach ($image in $images)
{    
    docker manifest create --amend $image `
      "$($image)-linux-arm64" `
      "$($image)-linux-amd64"
    
    docker manifest push $image
}