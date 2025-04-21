# Fill in your RabbitMQ server details
$rabbitMqHost = "localhost"
$rabbitMqPort = "15672"  # Management plugin port
$rabbitMqVhost = "/"  # default vhost
$rabbitMqUserName = "guest"
$rabbitMqPassword = "guest"

$queues = @(
    "customQueue" ##,
    ## Add more queues as needed
)


foreach($queue in $queues) {

    # Fill in the queue details
    $queueName = $queue
    $isDurable = $true
    $isExclusive = $false
    $isAutoDelete = $false

    # Prepare the queue properties
    $queueProperties = @{
        durable     = $isDurable
        exclusive   = $isExclusive
        auto_delete = $isAutoDelete
    } | ConvertTo-Json

    # Prepare the authorization headers
    $authorizationBytes = [System.Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $rabbitMqUserName, $rabbitMqPassword))
    $authorizationBase64 = [System.Convert]::ToBase64String($authorizationBytes)
    $headers = @{
        Authorization = ("Basic {0}" -f $authorizationBase64)
    }

    # Construct the URI
    $uri = ("http://{0}:{1}/api/queues/{2}/{3}" -f $rabbitMqHost, $rabbitMqPort, [System.Web.HttpUtility]::UrlEncode($rabbitMqVhost), $queueName)

    # Send the API request
    Invoke-RestMethod -Method Put -Uri $uri -Body $queueProperties -ContentType "application/json" -Headers $headers

    Write-Output "Queue '$uri' created with parameters: $queueProperties"
}

