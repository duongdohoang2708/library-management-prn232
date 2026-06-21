$apiKey = "AQ.Ab8RN6IPak8RpJP1PoeRzwmgOcf6xU0SMzD2Q5g9m5NU_r_plA"
$uri = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=$apiKey"
$body = @{
    contents = @(
        @{
            parts = @(
                @{ text = "Hello" }
            )
        }
    )
} | ConvertTo-Json -Depth 5

try {
    $response = Invoke-RestMethod -Uri $uri -Method Post -Body $body -ContentType "application/json"
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Output "Error: $_"
    $stream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($stream)
    $reader.ReadToEnd()
}
