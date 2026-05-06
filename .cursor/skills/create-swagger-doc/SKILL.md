---
name: Create Swagger Doc
description: Enrich .NET (ASP.NET Core) controller actions with accurate OpenAPI/Swagger annotations — `[ProducesResponseType]`, `[SwaggerOperation]`, `[SwaggerResponse]`, `[Consumes]`/`[Produces]`, parameter-binding attributes, and XML doc comments — without altering business logic or response shapes. Use whenever the user asks to document, annotate, decorate, or "swagger" a controller, action, or endpoint, or mentions OpenAPI, Swashbuckle, NSwag, ProducesResponseType, SwaggerOperation, or "API docs" in a .NET/C# context. Also use proactively right after writing or modifying a controller endpoint that lacks complete documentation. Accepts a free-form scope from the invocation (e.g. "the new PUT endpoint, 200 and 404 only") and respects it.
---

# Create Swagger Doc

Produce annotations that are **accurate, complete for the declared scope, and behavior-preserving**. The goal is documentation that matches what the code actually does — not what it might do, not what it should do.

## Invocation pattern

This skill is typically called with a scope instruction, e.g.

> `/create-swagger-doc Enrich the new PUT endpoint with comprehensive OpenAPI/Swagger annotations. Add [ProducesResponseType] attributes for the 200 and 404 status codes, and a [SwaggerOperation] description.`

Treat the instruction as authoritative for **which endpoint** and **which outcomes** to document. If the user names specific status codes, document exactly those — do not silently add others. If the user says "comprehensive", apply the full workflow below.

## Workflow

1. **Locate the action(s) in scope** from the instruction or the most recently modified controller file.
2. **Trace every return path** — explicit `return Ok(...)`, `return NotFound()`, `return BadRequest(...)`, `return CreatedAtAction(...)`, model-validation short-circuits (`ApiController` auto-400), `[Authorize]` (401/403), exception filters that translate to status codes, and any `ProblemDetails` responses.
3. **Map each return to a status code and a response type.** For success, the concrete DTO. For errors, typically `ProblemDetails` or `ValidationProblemDetails` if the project follows RFC 7807; otherwise the project's error DTO.
4. **Apply the attributes** in this order above the method: routing → `[Consumes]`/`[Produces]` → `[SwaggerOperation]` → `[ProducesResponseType]` (one per status, ascending). Add `[FromRoute]`/`[FromBody]`/`[FromQuery]` to parameters where the source isn't already obvious.
5. **Add XML doc comments** (`<summary>`, `<param>`, `<response code="...">`) — Swashbuckle surfaces these in Swagger UI when `IncludeXmlComments` is configured. They complement attributes; they don't replace them.
6. **Validate** against the checklist below before returning.

## Attribute reference

| Purpose                      | Attribute                                                                                                     |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------- |
| Response type + status       | `[ProducesResponseType(typeof(T), StatusCodes.Status200OK)]`                                                  |
| Status only (no body)        | `[ProducesResponseType(StatusCodes.Status204NoContent)]`                                                      |
| Endpoint summary/description | `[SwaggerOperation(Summary = "...", Description = "...", OperationId = "...", Tags = new[] { "..." })]`       |
| Response with description    | `[SwaggerResponse(200, "OK", typeof(T))]` (alternative to `ProducesResponseType` when you want a description) |
| Content type produced        | `[Produces("application/json")]`                                                                              |
| Content type consumed        | `[Consumes("application/json")]`                                                                              |
| Parameter binding source     | `[FromBody]`, `[FromRoute]`, `[FromQuery]`, `[FromHeader]`, `[FromForm]`                                      |
| Group / version              | `[ApiExplorerSettings(GroupName = "v1")]`                                                                     |
| Hide from docs               | `[ApiExplorerSettings(IgnoreApi = true)]`                                                                     |

Use `StatusCodes.Status*` constants — never magic numbers like `200`. Use `typeof(ConcreteDto)` — never `typeof(object)` or `typeof(IActionResult)`.

## Status code conventions by HTTP verb

Document only what the action actually returns. The columns below are the _candidate_ set, not a checklist to apply blindly.

| Verb                  | Typical success                          | Typical errors                           |
| --------------------- | ---------------------------------------- | ---------------------------------------- |
| `GET` (item)          | `200 OK` with body                       | `400`, `401`, `403`, `404`               |
| `GET` (list)          | `200 OK` with collection                 | `400` (bad filter), `401`, `403`         |
| `POST` (create)       | `201 Created` + `Location` header + body | `400`, `401`, `403`, `409`, `422`        |
| `POST` (action / RPC) | `200 OK` or `202 Accepted`               | `400`, `401`, `403`, `409`, `422`        |
| `PUT` (replace)       | `200 OK` with body, or `204 No Content`  | `400`, `401`, `403`, `404`, `409`        |
| `PATCH` (partial)     | `200 OK` or `204 No Content`             | `400`, `401`, `403`, `404`, `409`, `422` |
| `DELETE`              | `204 No Content` (preferred)             | `401`, `403`, `404`, `409`               |

`401` and `403` are documented only when `[Authorize]` (or a derived policy attribute) actually applies to the action or controller. Don't add them speculatively.

## Worked example — PUT endpoint

**Before**

```csharp
[HttpPut("{id}")]
public async Task<IActionResult> UpdateProduct(int id, ProductUpdateDto dto)
{
    var product = await _service.GetById(id);
    if (product is null) return NotFound();
    await _service.Update(id, dto);
    return Ok(product);
}
```

**After** (matching the invocation: 200 + 404 + `SwaggerOperation`)

```csharp
/// <summary>Updates an existing product.</summary>
/// <param name="id">The product identifier.</param>
/// <param name="dto">The updated product data.</param>
/// <response code="200">Product successfully updated; returns the updated representation.</response>
/// <response code="404">No product exists with the given <paramref name="id"/>.</response>
[HttpPut("{id}")]
[Consumes("application/json")]
[Produces("application/json")]
[SwaggerOperation(
    Summary     = "Update a product",
    Description = "Replaces the product identified by {id} with the values in the request body.",
    OperationId = "UpdateProduct",
    Tags        = new[] { "Products" })]
[ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> UpdateProduct(
    [FromRoute] int id,
    [FromBody]  ProductUpdateDto dto)
{
    var product = await _service.GetById(id);
    if (product is null) return NotFound();
    await _service.Update(id, dto);
    return Ok(product);
}
```

Note what did **not** change: the method body, the return type, the parameter types and order, and the routing. Documentation is purely additive.

## DTO documentation (when in scope)

If the request/response DTOs lack annotations and the user asked for "comprehensive" docs, also add:

- XML `<summary>` on the class and each public property.
- `[Required]`, `[StringLength]`, `[Range]`, `[RegularExpression]` to reflect existing validation logic — never to _introduce_ new validation.
- `[DefaultValue(...)]` for fields with documented defaults.
- `example` via `[SwaggerSchema(Description = "...")]` or `[DefaultValue]` where helpful.

If DTO changes risk altering serialization or validation behavior, leave them and call it out instead.

## Validation checklist

Before finishing, verify:

- [ ] Every `return` in the action maps to a documented status code.
- [ ] Every documented status code maps to an actual return path (no fiction).
- [ ] Response types are concrete DTOs, not `object` / `IActionResult`.
- [ ] Status codes use `StatusCodes.Status*` constants.
- [ ] `[SwaggerOperation]` `Summary` is a short action phrase (verb + object); `Description` adds detail without restating the summary.
- [ ] XML `<response>` text and `[ProducesResponseType]` status sets agree.
- [ ] Parameter binding sources are explicit where ambiguity is possible.
- [ ] `[Authorize]`-implied codes (401/403) are documented only if `[Authorize]` actually applies.
- [ ] Method body, return type, route, and parameter shapes are byte-identical to the original.
- [ ] If the invocation specified a status-code subset, only those were added.

## What to avoid

- Adding 401/403 to anonymous endpoints.
- Using `typeof(object)`, `typeof(IActionResult)`, or `typeof(ActionResult<T>)` — unwrap to the concrete DTO.
- Documenting outcomes the action never returns, even if "the API should support them".
- Changing return types, response shapes, routing, or parameter signatures.
- Removing or reordering existing functional attributes (`[HttpGet]`, `[Authorize]`, filters).
- Introducing new validation attributes on DTOs that change runtime behavior.
- Replacing existing XML comments with attribute-only docs (or vice versa) — they coexist.
