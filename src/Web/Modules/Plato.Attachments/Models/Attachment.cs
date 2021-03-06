﻿using System;
using System.Data;
using PlatoCore.Abstractions;
using PlatoCore.Abstractions.Extensions;
using PlatoCore.Models.Users;

namespace Plato.Attachments.Models
{

    public class Attachment : IDbModel
    {

        public int Id { get; set; }

        public string Name { get; set; }

        public byte[] ContentBlob { get; set; }

        public string ContentType { get; set; }

        public long ContentLength { get; set; }

        public string ContentGuid { get; set; }

        public string ContentCheckSum { get; set; }

        public int TotalViews { get; set; }

        public DateTimeOffset? CreatedDate { get; set; }

        public int CreatedUserId { get; set; }

        public ISimpleUser CreatedBy { get; set; }

        public int ModifiedUserId { get; set; }

        public DateTimeOffset? ModifiedDate { get; set; }

        public ISimpleUser ModifiedBy { get; set; }

        public virtual void PopulateModel(IDataReader dr)
        {

            if (dr.ColumnIsNotNull("id"))
                Id = Convert.ToInt32(dr["Id"]);
            
            if (dr.ColumnIsNotNull("Name"))
                Name = Convert.ToString(dr["Name"]);

            if (dr.ColumnIsNotNull("ContentBlob"))
                ContentBlob = (byte[])(dr["ContentBlob"]);

            if (dr.ColumnIsNotNull("ContentType"))
                ContentType = Convert.ToString(dr["ContentType"]);

            if (dr.ColumnIsNotNull("ContentLength"))
                this.ContentLength = Convert.ToInt64(dr["ContentLength"]);

            if (dr.ColumnIsNotNull("ContentGuid"))
                ContentGuid = Convert.ToString(dr["ContentGuid"]);

            if (dr.ColumnIsNotNull("ContentCheckSum"))
                ContentCheckSum = Convert.ToString(dr["ContentCheckSum"]);

            if (dr.ColumnIsNotNull("TotalViews"))
                TotalViews = Convert.ToInt32(dr["TotalViews"]);

            if (dr.ColumnIsNotNull("CreatedUserId"))
                CreatedUserId = Convert.ToInt32(dr["CreatedUserId"]);

            if (dr.ColumnIsNotNull("CreatedDate"))
                CreatedDate = (DateTimeOffset)dr["CreatedDate"];

            if (dr.ColumnIsNotNull("ModifiedUserId"))
                ModifiedUserId = Convert.ToInt32(dr["ModifiedUserId"]);

            if (dr.ColumnIsNotNull("ModifiedDate"))
                ModifiedDate = (DateTimeOffset)dr["ModifiedDate"];

        }

    }

}