﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Plato.Docs.Models;
using Plato.Entities.Extensions;
using PlatoCore.Models.Users;
using PlatoCore.Navigation.Abstractions;
using PlatoCore.Security.Abstractions;

namespace Plato.Docs.Navigation
{
    public class DocMenu : INavigationProvider
    {
        
        public IStringLocalizer T { get; set; }

        public DocMenu(IStringLocalizer localizer)
        {
            T = localizer;
        }

        public void BuildNavigation(string name, INavigationBuilder builder)
        {

            if (!String.Equals(name, "doc", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Get model from context
            var entity = builder.ActionContext.HttpContext.Items[typeof(Doc)] as Doc;
            if (entity == null)
            {
                return;
            }
            
            // Get authenticated user from context
            var user = builder.ActionContext.HttpContext.Features[typeof(User)] as User;
            
            Permission deletePermission = null;
            if (entity.IsDeleted)
            {
                // Do we have restore permissions?
                deletePermission = user?.Id == entity.CreatedUserId
                    ? Permissions.RestoreOwnDocs
                    : Permissions.RestoreAnyDoc;
            }
            else
            {
                // Do we have delete permissions?
                deletePermission = user?.Id == entity.CreatedUserId
                    ? Permissions.DeleteOwnDocs
                    : Permissions.DeleteAnyDoc;
            }

            // Add topic options
            builder
                .Add(T["Options"], int.MaxValue, options => options
                        .IconCss("fa fa-ellipsis-h")
                        .Attributes(new Dictionary<string, object>()
                        {
                            {"data-provide", "tooltip"},
                            {"title", T["Options"]}
                        })
                        .Add(T["Edit"], int.MinValue, edit => edit
                            .Action("Edit", "Home", "Plato.Docs", new RouteValueDictionary()
                            {
                                ["opts.id"] = entity.Id,
                                ["opts.alias"] = entity.Alias
                            })
                            .Permission(user?.Id == entity.CreatedUserId
                                ? Permissions.EditOwnDocs
                                : Permissions.EditAnyDoc)
                            .LocalNav()
                        )
                          .Add(entity.IsPinned ? T["Unpin"] : T["Pin"], 1, edit => edit
                            .Action(entity.IsPinned ? "Unpin" : "Pin", "Home", "Plato.Docs",
                                new RouteValueDictionary()
                                {
                                    ["id"] = entity.Id
                                })
                            .Resource(entity.CategoryId)
                            .Permission(entity.IsPinned
                                ? Permissions.UnpinDocs
                                : Permissions.PinDocs)
                            .LocalNav()
                        )
                        .Add(entity.IsLocked ? T["Unlock"] : T["Lock"], 2, edit => edit
                            .Action(entity.IsLocked ? "Unlock" : "Lock", "Home", "Plato.Docs",
                                new RouteValueDictionary()
                                {
                                    ["id"] = entity.Id
                                })
                            .Resource(entity.CategoryId)
                            .Permission(entity.IsLocked
                                ? Permissions.UnlockDocs
                                : Permissions.LockDocs)
                            .LocalNav()
                        )
                        .Add(entity.IsHidden ? T["Unhide"] : T["Hide"], 2, edit => edit
                            .Action(entity.IsHidden ? "Show" : "Hide", "Home", "Plato.Docs",
                                new RouteValueDictionary()
                                {
                                    ["id"] = entity.Id
                                })
                            .Resource(entity.CategoryId)
                            .Permission(entity.IsHidden
                                ? Permissions.ShowDocs
                                : Permissions.HideDocs)
                            .LocalNav()
                        )
                        .Add(entity.IsSpam ? T["Not Spam"] : T["Spam"], 2, spam => spam
                            .Action(entity.IsSpam ? "FromSpam" : "ToSpam", "Home", "Plato.Docs",
                                new RouteValueDictionary()
                                {
                                    ["id"] = entity.Id
                                })
                            .Resource(entity.CategoryId)
                            .Permission(entity.IsSpam
                                ? Permissions.DocFromSpam
                                : Permissions.DocToSpam)
                            .LocalNav()
                        )
                        .Add(T["Report"], int.MaxValue - 2, report => report
                            .Action("Report", "Home", "Plato.Docs", new RouteValueDictionary()
                            {
                                ["opts.id"] = entity.Id,
                                ["opts.alias"] = entity.Alias
                            })
                            .Attributes(new Dictionary<string, object>()
                            {
                                {"data-provide", "dialog"},
                                {"data-dialog-modal-css", "modal fade"},
                                {"data-dialog-css", "modal-dialog modal-lg"}
                            })
                            .Permission(Permissions.ReportDocs)
                            .LocalNav()
                        )
                        .Add(T["Divider"], int.MaxValue - 1, divider => divider
                            .Permission(deletePermission)
                            .DividerCss("dropdown-divider").LocalNav()
                        )
                        .Add(entity.IsDeleted ? T["Restore"] : T["Delete"], int.MaxValue, edit => edit
                                .Attributes(!entity.IsDeleted
                                    ? new Dictionary<string, object>()
                                    {
                                        ["data-provide"] = "confirm",
                                        ["data-confirm-message"] = "Are you sure you want to delete this doc?\n\nClick OK to confirm..."
                                    }
                                    : new Dictionary<string, object>())
                                .Action(entity.IsDeleted ? "Restore" : "Delete", "Home", "Plato.Docs",
                                    new RouteValueDictionary()
                                    {
                                        ["id"] = entity.Id
                                    })
                                .Permission(deletePermission)
                                .LocalNav(),
                            entity.IsDeleted
                                ? new List<string>() {"dropdown-item", "dropdown-item-success"}
                                : new List<string>() {"dropdown-item", "dropdown-item-danger"}
                        )
                    , new List<string>() {"doc-options", "text-muted", "dropdown-toggle-no-caret", "text-hidden"}
                );

            // If the entity if deleted display permanent delete option
            if (entity.IsDeleted)
            {

                // Permanent delete permissions
                var permanentDeletePermission = entity.CreatedUserId == user?.Id
                    ? Permissions.PermanentDeleteOwnDocs
                    : Permissions.PermanentDeleteAnyDoc;

                builder
                    .Add(T["Delete"], int.MinValue, options => options
                            .IconCss("fal fa-trash-alt")
                            .Attributes(new Dictionary<string, object>()
                            {
                                {"data-toggle", "tooltip"},
                                {"data-provide", "confirm"},
                                {"title", T["Permanent Delete"]},
                            })
                            .Action("PermanentDelete", "Home", "Plato.Docs",
                                new RouteValueDictionary()
                                {
                                    ["id"] = entity.Id.ToString(),
                                })
                            .Permission(permanentDeletePermission)
                            .LocalNav()
                        , new List<string>() { "doc-permanent-delete", "text-muted", "text-hidden" }
                    );
            }


            // If entity is not hidden or locked allow replies
            //if (!entity.IsHidden() && !entity.IsLocked)
            //{
            //    builder
            //        .Add(T["Reply"], int.MaxValue, options => options
            //                .IconCss("fa fa-reply")
            //                .Attributes(new Dictionary<string, object>()
            //                {
            //                    {"data-provide", "postReply"},
            //                    {"data-toggle", "tooltip"},
            //                    {"title", T["Comment"]}
            //                })
            //                .Action("Login", "Account", "Plato.Users",
            //                    new RouteValueDictionary()
            //                    {
            //                        ["returnUrl"] = builder.ActionContext.HttpContext.Request.Path
            //                    })
            //                .Permission(Permissions.PostDocComments)
            //                .LocalNav()
            //            , new List<string>() {"doc-reply", "text-muted", "text-hidden"}
            //        );

            //}

        }

    }

}
