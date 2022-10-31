using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using EdlinSoftware.Safe.Domain;
using EdlinSoftware.Safe.Domain.Model;
using Newtonsoft.Json;

namespace EdlinSoftware.Safe.Services;

public class ExportService
{
    private readonly IItemsRepository _itemsRepository;

    public ExportService(IItemsRepository itemsRepository)
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
}

public class ExportItem
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string[]? Tags { get; set; }
    public ExportField[]? Fields { get; set; }
    public ExportItem[]? SubItems { get; set; }
}

public abstract class ExportField
{
    public string Name { get; set; } = string.Empty;
}

public sealed class ExportTextField : ExportField
{
    public string? Text { get; set; }
}

public sealed class ExportPasswordField : ExportField
{
    public string? Password { get; set; }
}