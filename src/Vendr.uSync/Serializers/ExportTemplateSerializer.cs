﻿using System;
using System.Xml.Linq;

using Umbraco.Core.Logging;

using uSync8.Core;
using uSync8.Core.Extensions;
using uSync8.Core.Models;
using uSync8.Core.Serialization;

using Vendr.Core;
using Vendr.Core.Api;
using Vendr.Core.Models;
using Vendr.uSync.Extensions;

namespace Vendr.uSync.Serializers
{
    [SyncSerializer("6D4C64D0-B840-47F7-AF92-61A1C86D892E", "Export Template Serializer", VendrConstants.Serialization.ExportTemplate)]
    public class ExportTemplateSerializer : VendrSerializerBase<ExportTemplateReadOnly>, ISyncSerializer<ExportTemplateReadOnly>
    {
        public ExportTemplateSerializer(IVendrApi vendrApi, IUnitOfWorkProvider uowProvider, ILogger logger)
            : base(vendrApi, uowProvider, logger)
        { }

        protected override SyncAttempt<XElement> SerializeCore(ExportTemplateReadOnly item, SyncSerializerOptions options)
        {
            var node = InitializeBaseNode(item, ItemAlias(item));

            node.Add(new XElement(nameof(item.Name), item.Name));
            node.Add(new XElement(nameof(item.SortOrder), item.SortOrder));
            node.AddStoreId(item.StoreId);

            node.Add(new XElement(nameof(item.Category), item.Category));
            node.Add(new XElement(nameof(item.FileMimeType), item.FileMimeType));
            node.Add(new XElement(nameof(item.FileExtension), item.FileExtension));
            node.Add(new XElement(nameof(item.ExportStrategy), item.ExportStrategy));
            node.Add(new XElement(nameof(item.TemplateView), item.TemplateView));

            return SyncAttempt<XElement>.SucceedIf(node != null, item.Name, node, ChangeType.Export);
        }

        public override bool IsValid(XElement node)
            => base.IsValid(node)
            && node.GetStoreId() != Guid.Empty;

        protected override SyncAttempt<ExportTemplateReadOnly> DeserializeCore(XElement node, SyncSerializerOptions options)
        {
            var readOnlyItem = FindItem(node);

            var alias = node.GetAlias();
            var id = node.GetKey();
            var name = node.Element(nameof(readOnlyItem.Name)).ValueOrDefault(alias);
            var storeId = node.GetStoreId();

            using (var uow = _uowProvider.Create())
            {
                ExportTemplate item;
                if (readOnlyItem == null)
                {
                    item = ExportTemplate.Create(uow, id, storeId, alias, name);
                }
                else
                {
                    item = readOnlyItem.AsWritable(uow);
                    item.SetAlias(alias)
                         .SetName(name);
                }

                item.SetCategory(node.Element(nameof(item.Category)).ValueOrDefault(item.Category));
                item.SetFileMimeType(node.Element(nameof(item.FileMimeType)).ValueOrDefault(item.FileMimeType));
                item.SetFileExtension(node.Element(nameof(item.FileExtension)).ValueOrDefault(item.FileExtension));
                item.SetExportStrategy(node.Element(nameof(item.ExportStrategy)).ValueOrDefault(item.ExportStrategy));
                item.SetTemplateView(node.Element(nameof(item.TemplateView)).ValueOrDefault(item.TemplateView));

                _vendrApi.SaveExportTemplate(item);

                uow.Complete();

                return SyncAttempt<ExportTemplateReadOnly>.Succeed(name, item.AsReadOnly(), ChangeType.Import);
            }
        }

        // 

        protected override void DeleteItem(ExportTemplateReadOnly item)
            => _vendrApi.DeleteExportTemplate(item.Id);

        protected override ExportTemplateReadOnly FindItem(Guid key)
            => _vendrApi.GetExportTemplate(key);

        protected override ExportTemplateReadOnly FindItem(string alias)
            => null;

        protected override string ItemAlias(ExportTemplateReadOnly item)
            => item.Alias;

        protected override void SaveItem(ExportTemplateReadOnly item)
        {
            using (var uow = _uowProvider.Create())
            {
                var entity = item.AsWritable(uow);
                _vendrApi.SaveExportTemplate(entity);
                uow.Complete();
            }
        }
    }
}
