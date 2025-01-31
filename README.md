# NJsonSchema for .NET

[NSwag](http://nswag.org) | NJsonSchema | [Apimundo](https://apimundo.com) | [Namotion.Reflection](https://github.com/RicoSuter/Namotion.Reflection)

[![Azure DevOps](https://img.shields.io/azure-devops/build/rsuter/NJsonSchema/17/master.svg)](https://rsuter.visualstudio.com/NJsonSchema/_build?definitionId=17)
[![Nuget](https://img.shields.io/nuget/v/NJsonSchema.svg)](https://www.nuget.org/packages?q=NJsonSchema)
[![Discord](https://img.shields.io/badge/Discord-join%20chat-1dce73.svg)](https://discord.gg/BxQNy25WF6)
[![StackOverflow](https://img.shields.io/badge/questions-on%20StackOverflow-orange.svg?style=flat)](http://stackoverflow.com/questions/tagged/njsonschema)
[![Wiki](https://img.shields.io/badge/docs-in%20wiki-orange.svg?style=flat)](https://github.com/RicoSuter/njsonschema/wiki)
[![Apimundo](https://img.shields.io/badge/Architecture-Apimundo-728199.svg)](https://apimundo.com/organizations/github/projects/ricosuter?tab=repositories)

<img align="left" src="https://raw.githubusercontent.com/RSuter/NJsonSchema/master/assets/GitHubIcon.png">

NJsonSchema is a .NET library to read, generate and validate JSON Schema draft v4+ schemas. The library can read a schema from a file or string and validate JSON data against it. A schema can also be generated from an existing .NET class. With the code generation APIs you can generate C# and TypeScript classes or interfaces from a schema. 

The library uses [Json.NET](http://james.newtonking.com/json) to read and write JSON data and [Namotion.Reflection](https://github.com/RicoSuter/Namotion.Reflection) for additional .NET reflection APIs.

**NuGet packages:** 
- [NJsonSchema](https://apimundo.com/organizations/nuget-org/nuget-feeds/public/packages/NJsonSchema/versions/latest) : JSON Schema parsing, validation and generation classes
- [NJsonSchema.Annotations](https://apimundo.com/organizations/nuget-org/nuget-feeds/public/packages/NJsonSchema.Annotations/versions/latest) : JSON Schema annotations controlling serialization
- [NJsonSchema.Yaml](https://apimundo.com/organizations/nuget-org/nuget-feeds/public/packages/NJsonSchema.Yaml/versions/latest) : Read and write JSON Schemas from YAML
- [NJsonSchema.CodeGeneration](https://apimundo.com/organizations/nuget-org/nuget-feeds/public/packages/NJsonSchema.CodeGeneration/versions/latest) : Base classes to generate code from a JSON Schema
- [NJsonSchema.CodeGeneration.CSharp](https://apimundo.com/organizations/nuget-org/nuget-feeds/public/packages/NJsonSchema.CodeGeneration.CSharp/versions/latest) : Generates CSharp classes
- [NJsonSchema.CodeGeneration.TypeScript](https://apimundo.com/organizations/nuget-org/nuget-feeds/public/packages/NJsonSchema.CodeGeneration.TypeScript/versions/latest) : Generates TypeScript interfaces or classes

Preview NuGet Feed: https://www.myget.org/F/njsonschema/api/v3/index.json

**Features:**

- [Read existing JSON Schemas](https://github.com/RicoSuter/NJsonSchema/wiki/JsonSchema) and [validate JSON data](https://github.com/RicoSuter/NJsonSchema/wiki/JsonSchemaValidator) (`JsonSchema.FromJsonAsync()`)
- [Generate JSON Schema from .NET type via reflection](https://github.com/RicoSuter/NJsonSchema/wiki/JsonSchemaGenerator) (with support for many attributes/annotations) (`JsonSchema.FromType<MyType>()`)
- [Generate JSON Schema from sample JSON data](https://github.com/RicoSuter/NJsonSchema/wiki/SampleJsonSchemaGenerator) (`JsonSchema.FromSampleJson()`)
- Support for schema references ($ref) (relative, URL and file)
- Generate C# and TypeScript code from JSON Schema
- Supports .NET Standard 2.0, also see [XML Documentation](https://github.com/NJsonSchema/NJsonSchema/wiki/XML-Documentation))
- Supports JSON Schema, Swagger and OpenAPI DTO schemas

NJsonSchema is heavily used in [NSwag](http://nswag.org), a Swagger API toolchain for .NET which generates client code for Web API services. NSwag also provides command line tools to use the NJsonSchema's JSON Schema generator (command `types2swagger`). 

The project is developed and maintained by [Rico Suter](http://rsuter.com) and other contributors. 

**Some code generators can directly be used via the [Apimundo service](https://apimundo.com/tools).**

## NJsonSchema usage

The [JsonSchema](https://github.com/NJsonSchema/NJsonSchema/wiki/JsonSchema) class can be used as follows: 

```csharp
var schema = JsonSchema.FromType<Person>();
var schemaData = schema.ToJson();
var errors = schema.Validate("{...}");

foreach (var error in errors)
    Console.WriteLine(error.Path + ": " + error.Kind);

schema = await JsonSchema.FromJsonAsync(schemaData);
```

The `Person` class: 

```cs
public class Person
{
    [Required]
    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    [Required]
    public string LastName { get; set; }

    public Gender Gender { get; set; }

    [Range(2, 5)]
    public int NumberWithRange { get; set; }

    public DateTime Birthday { get; set; }

    public Company Company { get; set; }

    public Collection<Car> Cars { get; set; }
}

public enum Gender
{
    Male,
    Female
}

public class Car
{
    public string Name { get; set; }

    public Company Manufacturer { get; set; }
}

public class Company
{
    public string Name { get; set; }
}
```
  
The generated JSON schema data stored in the `schemaData` variable: 

```json
{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "title": "Person",
  "type": "object",
  "additionalProperties": false,
  "required": [
    "FirstName",
    "LastName"
  ],
  "properties": {
    "FirstName": {
      "type": "string"
    },
    "MiddleName": {
      "type": [
        "null",
        "string"
      ]
    },
    "LastName": {
      "type": "string"
    },
    "Gender": {
      "oneOf": [
        {
          "$ref": "#/definitions/Gender"
        }
      ]
    },
    "NumberWithRange": {
      "type": "integer",
      "format": "int32",
      "maximum": 5.0,
      "minimum": 2.0
    },
    "Birthday": {
      "type": "string",
      "format": "date-time"
    },
    "Company": {
      "oneOf": [
        {
          "$ref": "#/definitions/Company"
        },
        {
          "type": "null"
        }
      ]
    },
    "Cars": {
      "type": [
        "array",
        "null"
      ],
      "items": {
        "$ref": "#/definitions/Car"
      }
    }
  },
  "definitions": {
    "Gender": {
      "type": "integer",
      "description": "",
      "x-enumNames": [
        "Male",
        "Female"
      ],
      "enum": [
        0,
        1
      ]
    },
    "Company": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "Name": {
          "type": [
            "null",
            "string"
          ]
        }
      }
    },
    "Car": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "Name": {
          "type": [
            "null",
            "string"
          ]
        },
        "Manufacturer": {
          "oneOf": [
            {
              "$ref": "#/definitions/Company"
            },
            {
              "type": "null"
            }
          ]
        }
      }
    }
  }
}
```

## NJsonSchema.CodeGeneration usage

The `NJsonSchema.CodeGeneration` can be used to generate C# or TypeScript code from a JSON schema:

```cs
var generator = new CSharpGenerator(schema);
var file = generator.GenerateFile();
```
    
The `file` variable now contains the C# code for all the classes defined in the JSON schema. 

### TypeScript

The previously generated JSON Schema would generate the following TypeScript interfaces. 

**Settings:** 

    new TypeScriptGeneratorSettings { TypeStyle = TypeScriptTypeStyle.Interface, TypeScriptVersion = 2.0m }
    
**Output:** 

```typescript
export enum Gender {
    Male = 0, 
    Female = 1, 
}

export interface Company {
    Name: string | undefined;
}

export interface Car {
    Name: string | undefined;
    Manufacturer: Company | undefined;
}

export interface Person {
    FirstName: string;
    MiddleName: string | undefined;
    LastName: string;
    Gender: Gender;
    NumberWithRange: number;
    Birthday: Date;
    Company: Company | undefined;
    Cars: Car[] | undefined;
}
```

... and the following TypeScript classes. 

**Settings:** 

    new TypeScriptGeneratorSettings { TypeStyle = TypeScriptTypeStyle.Class, TypeScriptVersion = 2.0m }

**Output:**

```typescript
export enum Gender {
    Male = 0, 
    Female = 1, 
}

export class Company implements ICompany {
    name: string | undefined;

    constructor(data?: ICompany) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["Name"];
        }
    }

    static fromJS(data: any): Company {
        let result = new Company();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["Name"] = this.name;
        return data; 
    }
}

export interface ICompany {
    name: string | undefined;
}

export class Car implements ICar {
    name: string | undefined;
    manufacturer: Company | undefined;

    constructor(data?: ICar) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["Name"];
            this.manufacturer = data["Manufacturer"] ? Company.fromJS(data["Manufacturer"]) : <any>undefined;
        }
    }

    static fromJS(data: any): Car {
        let result = new Car();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["Name"] = this.name;
        data["Manufacturer"] = this.manufacturer ? this.manufacturer.toJSON() : <any>undefined;
        return data; 
    }
}

export interface ICar {
    name: string | undefined;
    manufacturer: Company | undefined;
}

export class Person implements IPerson {
    firstName: string;
    middleName: string | undefined;
    lastName: string;
    gender: Gender;
    numberWithRange: number;
    birthday: Date;
    company: Company | undefined;
    cars: Car[] | undefined;

    constructor(data?: IPerson) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.firstName = data["FirstName"];
            this.middleName = data["MiddleName"];
            this.lastName = data["LastName"];
            this.gender = data["Gender"];
            this.numberWithRange = data["NumberWithRange"];
            this.birthday = data["Birthday"] ? new Date(data["Birthday"].toString()) : <any>undefined;
            this.company = data["Company"] ? Company.fromJS(data["Company"]) : <any>undefined;
            if (data["Cars"] && data["Cars"].constructor === Array) {
                this.cars = [];
                for (let item of data["Cars"])
                    this.cars.push(Car.fromJS(item));
            }
        }
    }

    static fromJS(data: any): Person {
        let result = new Person();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["FirstName"] = this.firstName;
        data["MiddleName"] = this.middleName;
        data["LastName"] = this.lastName;
        data["Gender"] = this.gender;
        data["NumberWithRange"] = this.numberWithRange;
        data["Birthday"] = this.birthday ? this.birthday.toISOString() : <any>undefined;
        data["Company"] = this.company ? this.company.toJSON() : <any>undefined;
        if (this.cars && this.cars.constructor === Array) {
            data["Cars"] = [];
            for (let item of this.cars)
                data["Cars"].push(item.toJSON());
        }
        return data; 
    }
}

export interface IPerson {
    firstName: string;
    middleName: string | undefined;
    lastName: string;
    gender: Gender;
    numberWithRange: number;
    birthday: Date;
    company: Company | undefined;
    cars: Car[] | undefined;
}
```

## NJsonSchema.SampleJsonSchemaGenerator usage

The `NJsonSchema.SampleJsonSchemaGenerator` can be used to generate a JSON Schema from sample JSON data:

### JSON Schema Specification

By default, the `NJsonSchema.SampleJsonSchemaGenerator` generates a JSON Schema based on the JSON Schema specification. See: [JSON Schema Specification](https://json-schema.org/specification) 

```csharp
var generator = new SampleJsonSchemaGenerator(new SampleJsonSchemaGeneratorSettings());

var schema = generator.Generate("{...}");
```

**Input:**

```json
{
  "int": 1,
  "float": 340282346638528859811704183484516925440.0,
  "str": "abc",
  "bool": true,
  "date": "2012-07-19",
  "datetime": "2012-07-19 10:11:11",
  "timespan": "10:11:11"
}
```

**Output:**

```json
{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "int": {
      "type": "integer"
    },
    "float": {
      "type": "number"
    },
    "str": {
      "type": "string"
    },
    "bool": {
      "type": "boolean"
    },
    "date": {
      "type": "string",
      "format": "date"
    },
    "datetime": {
      "type": "string",
      "format": "date-time"
    },
    "timespan": {
      "type": "string",
      "format": "duration"
    }
  }
}
```

### OpenApi Specification

To generate a JSON Schema for OpenApi, provide the `SchemaType.OpenApi3` in the settings. See: [OpenApi Specification](https://swagger.io/specification/)

```csharp
var generator = new SampleJsonSchemaGenerator(new SampleJsonSchemaGeneratorSettings { SchemaType = SchemaType.OpenApi3 });

var schema = generator.Generate("{...}");
```

**Input:**

```json
{
  "int": 12345,
  "long": 1736347656630,
  "float": 340282346638528859811704183484516925440.0,
  "double": 340282346638528859811704183484516925440123456.0,
}
```

**Output:**

```json
{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "int": {
      "type": "integer",
      "format": "int32"
    },
    "long": {
      "type": "integer",
      "format": "int64"
    },
    "float": {
      "type": "number",
      "format": "float"
    },
    "double": {
      "type": "number",
      "format": "double"
    }
  }
}
```

## Final notes

Applications which use the library: 

- [VisualJsonEditor](http://visualjsoneditor.org), a JSON schema based file editor for Windows. 
- [NSwag](http://nswag.org): The Swagger API toolchain for .NET
- [SigSpec for SignalR Core](https://github.com/RicoSuter/SigSpec): Specification and code generator for SignalR Core. 
