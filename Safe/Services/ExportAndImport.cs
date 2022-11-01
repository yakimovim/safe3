using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EdlinSoftware.Safe.Services;

public class ExportImportService
{
    private readonly IItemsRepository _itemsRepository;

    public ExportImportService(IItemsRepository itemsRepository)
    {
        _itemsRepository = itemsRepository ?? throw new ArgumentNullException(nameof(itemsRepository));
    }

    public bool Export(string targetFileName)
    {
        try
        {
            InternalExport(targetFileName);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void InternalExport(string targetFileName)
    {
        var exportItems = new LinkedList<ExportItem>();

        foreach (var item in _itemsRepository.GetChildItems(null))
        {
            exportItems.AddLast(CreateExportItem(item));
        }

        using var fileStream = File.OpenWrite(targetFileName);

        using var writer = new StreamWriter(fileStream);

        JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });

        serializer.Serialize(writer, exportItems);
    }

    private ExportItem CreateExportItem(Item item)
    {
        return new ExportItem
        {
            Title = item.Title,
            Description = item.Description,
            Tags = item.Tags.Any() ? item.Tags.ToArray() : null,
            Fields = item.Fields.Any() ? CreateExportFields(item.Fields) : null,
            SubItems = CreateSubItems(item)
        };
    }

    private ExportItem[]? CreateSubItems(Item item)
    {
        var subItems = _itemsRepository.GetChildItems(item);

        if (!subItems.Any()) return null;

        return subItems.Select(CreateExportItem).ToArray();
    }

    private ExportField[] CreateExportFields(IReadOnlyCollection<Field> fields)
    {
        return fields.Select(CreateExportField).ToArray();
    }

    private ExportField CreateExportField(Field field)
    {
        switch (field)
        {
            case TextField tf:
                return new ExportTextField
                {
                    Name = tf.Name,
                    Text = tf.Text
                };
            case PasswordField pf:
                return new ExportPasswordField
                {
                    Name = pf.Name,
                    Password = pf.Password
                };
            default:
                throw new InvalidOperationException($"Unknown field type {field.GetType().Name}");
        }
    }

    public void Import(string importFileName)
    {
        var importedItems = GetExportItems(importFileName);

        if(importedItems == null) return;

        var importRootItem = new Item
        {
            Title = "Import Root"
        };

        _itemsRepository.SaveItem(importRootItem);

        foreach (var importedItem in importedItems)
        {
            SaveImportedItem(importedItem, importRootItem);
        }
    }

    private void SaveImportedItem(ExportItem importedItem, Item parentItem)
    {
        var item = new Item(parentItem)
        {
            Title = importedItem.Title,
            Description = importedItem.Description,
        };

        if (importedItem.Tags != null)
        {
            item.Tags.AddRange(importedItem.Tags);
        }

        if (importedItem.Fields != null)
        {
            foreach (var importedField in importedItem.Fields)
            {
                switch (importedField)
                {
                    case ExportTextField tf:
                    {
                        item.Fields.Add(new TextField
                        {
                            Name = tf.Name,
                            Text = tf.Text ?? string.Empty
                        });
                        break;
                    }
                    case ExportPasswordField pf:
                    {
                        item.Fields.Add(new PasswordField()
                        {
                            Name = pf.Name,
                            Password = pf.Password ?? string.Empty
                        });
                        break;
                    }
                    default:
                        throw new InvalidOperationException($"Unknown type of field: '{importedField.GetType().Name}'");
                }
            }
        }

        _itemsRepository.SaveItem(item);

        if (importedItem.SubItems != null)
        {
            foreach (var importedSubItem in importedItem.SubItems)
            {
                SaveImportedItem(importedSubItem, item);
            }
        }
    }

    private IReadOnlyCollection<ExportItem>? GetExportItems(string importFileName)
    {
        try
        {
            using var fileStream = File.OpenRead(importFileName);

            using var reader = new StreamReader(fileStream);

            JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            return serializer.Deserialize<ExportItem[]>(new JsonTextReader(reader));
        }
        catch
        {
            return null;
        }
    }
}

public class ExportItem
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string[]? Tags { get; set; }
    public ExportField[]? Fields { get; set; }
    public ExportItem[]? SubItems { get; set; }
}

[JsonConverter(typeof(ExportFieldJsonConverter))]
public abstract class ExportField
{
    public string Name { get; set; } = string.Empty;
}

public class ExportFieldJsonConverter : JsonConverter<ExportField>
{
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, ExportField? value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public override ExportField? ReadJson(JsonReader reader, Type objectType, ExportField? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var jObject = JObject.Load(reader);

        ExportField? field = null;

        if (jObject.ContainsKey("Password"))
            field = new ExportPasswordField();
        else if(jObject.ContainsKey("Text"))
            field = new ExportTextField();

        if (field == null) return null;

        serializer.Populate(jObject.CreateReader(), field);

        return field;
    }
}

public sealed class ExportTextField : ExportField
{
    public string? Text { get; set; }
}

public sealed class ExportPasswordField : ExportField
{
    public string? Password { get; set; }
}