# generates product data in JSON for upload to Cosmos

$items=@()
for($i=0; $i -lt 1000; $i++) {
    $items += [pscustomobject]@{productId="$i";name="p$i";price=5*$i}
}
ConvertTo-Json $items > products.json

$refData=@()
$refData += [pscustomobject]@{refDataType='Products';items=$items}
ConvertTo-Json -Depth 5 $refData > refData.json