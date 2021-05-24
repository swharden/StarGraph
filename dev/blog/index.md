* rate limiting API
* V3 accept json
* number of pages is in response headers

```
<https://api.github.com/repositories/116210568/stargazers?page=2>; rel="next",
<https://api.github.com/repositories/116210568/stargazers?page=28>; rel="last"
```


request.Headers.Add("User-Agent", "request");