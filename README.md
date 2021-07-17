# Validação de entrada de dados e respostas de erro no ASP.NET

Nesse texto será discutido as formas de validação do ASP.NET e o formato das mensagens de erro retornadas. Serão tratados erros de tipos inválidos e a validação dos modelos usando a funcionalidade de DataAnnotations do ASP.NET. Além disso, será mostrado como customizar este formato. Os exemplos de código foram desenvolvidos usando a versão 5 do ASP.NET.

## Erros de Model Binding

[Model binding](https://docs.microsoft.com/pt-br/aspnet/core/mvc/models/model-binding?view=aspnetcore-5.0#what-is-model-binding) é a funcionalidade do ASP.NET que atua nas requisições HTTP convertendo os dados de entrada nas rotas em tipos .NET. Considere o `Controller` de exemplo:

```c#
[Route("[controller]")]
public class ExampleController : ControllerBase
{
    [HttpGet]
    public ActionResult Get(int id)
    {
        if (id == 1)
            return Ok(new ExampleRequest{Name = "Example1"});

        return NotFound();
    }
}
```

Ao fazer uma requisição para essa rota o ASP.NET vai examinar a requisição para encontrar o campo `id`, que nesse caso é enviada como parâmetro. Em seguida, o valor encontrado será convertido para inteiro e o método `Get` será executado. Mas o que acontece se o valor passado for um texto? O dado não é convertido e é preenchido com o valor padrão, que para inteiro é 0. Caso fosse um objeto, o valor padrão seria `null` o que poderia causar um `NullReferenceException` se não fosse feita nenhuma verificação.

### ModelState

A propriedade [ModelState](https://docs.microsoft.com/pt-br/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.modelstate?view=aspnetcore-5.0) da classe `ControllerBase` contém o estado do modelo e a validação de associação de modelo. Quando não é possível a conversão da entrada de dados o `ModelState` é inválido. Logo, é possível verificar se os dados de entrada estão corretos em relação aos tipos esperados.

```c#
[Route("[controller]")]
public class ExampleController : ControllerBase
{
    [HttpGet]
    public ActionResult Get(int id)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id == 1)
            return Ok(new ExampleRequest{Name = "Example1"});

        return NotFound();
    }
}
```

Dessa forma, se essa rota for chamada enviando o valor "texto" no campo `id` recebemos o seguinte erro:

```json
{
    "id": [
        "The value 'texto' is not valid."
    ]
}
```

Isso previne que a aplicação processe requisições com dados inválidos! Mas se minha aplicação possui vários controllers eu preciso repetir essa verificação em todos?


### [ApiController]

O atributo do ASP.NET [ApiController](https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-5.0#apicontroller-attribute) pode ser aplicado à Controllers e trás algumas funcionalidades. Entre elas, ele faz a validação automática dos dados de entrada e retorna um erro 400 de maneira similar à verificação do `ModelState`. Alterando o nosso Controller de exemplo:

```c#
[ApiController]
[Route("[controller]")]
public class ExampleController : ControllerBase
{
    [HttpGet]
    public ActionResult Get(int id)
    {
        if (id == 1)
            return Ok(new ExampleRequest{Name = "Example1"});

        return NotFound();
    }
}
```

E ao fazer a requisição com o valor inválido obtemos o mesmo erro quando usamos a verificação do `ModelState`. Além disso, o `ApiController` trás outras informações de erro no seu resultado:

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "traceId": "00-a89db4d9a02dfc479c4d50b401f60fb5-28ba31ad1392154c-00",
    "errors": {
        "id": [
            "The value 'texto' is not valid."
        ]
    }
}
```

Ou seja, por padrão o atributo apresenta o formato acima contendo:
- **type**: o link da RFC em que determina os tipos de resposta HTTP, especificamente para a sessão do erro 400;
- **status**: o código do status de erro;
- **traceId**: o traceId da requisição. Por padrão, o ASP.NET 5 utiliza o formato definido [pela recomendação da W3C](https://www.w3.org/TR/trace-context/). Você pode encontrar mais informações sobre o traceId e o trace context [aqui](https://dev.to/luizhlelis/using-w3c-trace-context-standard-in-distributed-tracing-3743);
- **errors**: uma lista de erros contendo o erro de validação do modelo.

## Validação do modelo

A validação dos tipos de dados é importante mas normalmente queremos aplicar outras validações ao nossos dados de entrada. Por exemplo, podemos marcar campos como obrigatórios, tamanho mínimo ou máximo e regras mais complexas. Ou seja, é importante garantir que a nossa aplicação só vai processar dados válidos. Isso também evita que o código da aplicação tenha uma quantidade de `if`s e `else`s que acabam poluindo o código. Vejam o exemplo abaixo:

```c#
[HttpPost]
public ActionResult Add(ExampleRequest example)
{
    return Ok();
}
...
public class ExampleRequest
{
    [Required]
    public string Name { get; set; }
}
```

O exemplo utiliza o atributo `Required` pressente no namespace [System.ComponentModel.DataAnnotations](https://docs.microsoft.com/pt-br/aspnet/core/mvc/models/validation?view=aspnetcore-5.0#validation-attributes). Existem vários outros atributos e também possível extender essa funcionalidade criando atributos customizados.

Realizando uma requisição para a nova rota de POST com o body vazio obtemos o erro:

```json
{
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "traceId": "00-421e7740cdb1394aba958b549d319bc2-ffc80b3fe2883349-00",
    "errors": {
        "Name": [
            "The Name field is required."
        ]
    }
}
```

Pode se notar que a resposta segue o mesmo padrão do erro por Model Binding.

O mesmo efeito pode ser obtido utilizando o [FluentValidation](https://docs.fluentvalidation.net/en/latest/index.html) configurando a sua [integração com o ASP.NET](https://docs.fluentvalidation.net/en/latest/aspnet.html).

## Customização da resposta de erro

Para alguns casos a resposta de erro padrão que o `ApiController` envia pode ser indequada para a aplicação. Por exemplo, os campos `status` e `type` são redundantes considerando que o código da resposta HTTP já é retornado na requisição. Além disso, caso a aplicação retorne outros tipos de erros 400 pode ser necessário incluir novos campos na resposta. Para isso o ASP.NET possui uma [funcionalidade](https://docs.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-5.0#validation-failure-error-response) que permite alterar o formato da resposta. Essa configuração deve ser feita no método `ConfigureServices` da classe `Startup` da aplicação. Deve ser chamado o método `ConfigureApiBehaviorOptions` preenchendo a propriedade `InvalidModelStateResponseFactory` com a customização da resposta. Segue o exemplo abaixo:

```c#
services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var response = new
            {
                Error = new Dictionary<string, string[]>(),
                Type = "VALIDATION_ERRORS"
            };
            foreach (var (key, value) in context.ModelState)
                response.Error.Add(key, value.Errors.Select(e => e.ErrorMessage).ToArray());

            return new BadRequestObjectResult(response);
        };
    });
```

O código acima simplifica o retorno da API, trazendo apenas a lista de erros e um novo campo para indicar que o motivo do erro é de validação dos dados. Fazendo novamente a requisição problemática recebemos o erro:

```json
{
    "error": {
        "Name": [
            "The Name field is required."
        ]
    },
    "type": "VALIDATION_ERRORS"
}
```

É importante notar que apenas erros de validação, ou seja, que o `ModelState` é ínválido, são afetados por essa customização.

## Conclusão

O ASP.NET possui funcionalidades para ajudar os desenvolvedores criarem APIs mais robustas aplicando validação de dados de entrada. Além disso, existem ótimas APIs como o Fluent Validation que permite mais liberdade para criação de validadores de modelos mais inteligentes. O uso do atributo `ApiController` do ASP.NET incrementa as APIs adicionando funcionalidades como a resposta 400 automática para erros de validação padrão. No entanto, se desejado, é possível customizar o resultado de forma simples usando o `InvalidModelStateResponseFactory`.


## Referências
https://docs.microsoft.com/pt-br/aspnet/core/mvc/models/model-binding?view=aspnetcore-5.0
https://docs.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-5.0
https://docs.microsoft.com/pt-br/aspnet/core/mvc/models/validation?view=aspnetcore-5.0
https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-5.0&tabs=visual-studio
https://docs.microsoft.com/pt-br/aspnet/core/web-api/handle-errors?view=aspnetcore-5.0
https://github.com/dotnet/aspnetcore/blob/52eff90fbcfca39b7eb58baad597df6a99a542b0/src/Mvc/Mvc.Core/src/Infrastructure/ModelStateInvalidFilter.cs