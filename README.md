### Related to issue  **"ASP.NET Core OpenAPI generator incorrectly duplicates schemas and produces invalid `$ref` for simple structure"**

### **Description**  
The OpenAPI generator in ASP.NET Core produces an incorrect schema when a DTO contains multiple properties of the same type.  
Instead of reusing the same schema, it creates a duplicate (`ChangesetIntDto2`). Additionally, it generates an **invalid `$ref`**, making the OpenAPI document **invalid and unusable**.

This occurs even in a **minimal** ASP.NET Core project using `Microsoft.Extensions.ApiDescription.Server` for OpenAPI generation.

---

### **Steps to reproduce**  
#### **1. Minimal `Program.cs`**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.UseHttpsRedirection();

app.MapGet("/repro", () =>
{
    return Enumerable.Range(1, 5).Select(_ =>
        new ChangesetDescDto
        {
            Prop1 = new ChangesetIntDto { Added = [] },
            Prop2 = new ChangesetIntDto { Added = [] },
        }).ToArray();
}).WithName("reproendpoint");

app.Run();
```

#### **2. DTOs**
```csharp
public sealed class ChangesetDescDto
{
    [JsonPropertyName("value1")]
    public required ChangesetIntDto Prop1 { get; init; }

    [JsonPropertyName("value2")]
    public required ChangesetIntDto Prop2 { get; init; }
}

public sealed class ChangesetIntDto
{
    [JsonPropertyName("added")]
    public required List<int> Added { get; init; }
}
```

#### **3. Project Configuration (`.csproj`)**
Using `Microsoft.Extensions.ApiDescription.Server` with OpenAPI document generation:  
```xml
<!-- For build time OpenAPI JSON generation -->
<PropertyGroup>
    <OpenApiDocumentsDirectory>.</OpenApiDocumentsDirectory>
    <OpenApiGenerateDocumentsOptions>--file-name repro-open-api</OpenApiGenerateDocumentsOptions>
</PropertyGroup>
```

#### **4. Generate OpenAPI JSON**
After running the project and generating the OpenAPI document, the following incorrect schema is produced.

---

### **Expected behavior**  
- Both `value1` and `value2` should reference `ChangesetIntDto`.  
- No duplicate schema (`ChangesetIntDto2`).  
- No invalid `$ref` values.  

### **Actual behavior**  
- `value1` correctly references `ChangesetIntDto`, but `value2` references `ChangesetIntDto2` (a duplicated schema).  
- An invalid `$ref` is generated:  
  ```json
  "$ref": "#/components/schemas/#/items/properties/value1/properties/added"
  ```

---

### **Example of incorrect OpenAPI output**  
```json
"components": {
  "schemas": {
    "ChangesetDescDto": {
      "required": ["value1", "value2"],
      "type": "object",
      "properties": {
        "value1": {
          "$ref": "#/components/schemas/ChangesetIntDto"
        },
        "value2": {
          "$ref": "#/components/schemas/ChangesetIntDto2"
        }
      }
    },
    "ChangesetIntDto": {
      "required": ["added"],
      "type": "object",
      "properties": {
        "added": {
          "type": "array",
          "items": {
            "type": "integer",
            "format": "int32"
          }
        }
      }
    },
    "ChangesetIntDto2": {
      "required": ["added"],
      "type": "object",
      "properties": {
        "added": {
          "$ref": "#/components/schemas/#/items/properties/value1/properties/added"
        }
      }
    }
  }
}
```

---

### **Annoyance level :-)**  
- **Breaks OpenAPI validation** due to an invalid `$ref` entry.  
- **Makes the schema unusable** for client code generation (TypeScript, C#).  
- **Introduces unnecessary schema duplication**, which is inefficient and confusing.  

---

### **Environment**  
- .NET 9
- ASP.NET Core
- OpenAPI generation using `builder.Services.AddOpenApi()`
- `Microsoft.Extensions.ApiDescription.Server`
- **Build-time OpenAPI document generation enabled in `.csproj`**

---

Thanks.