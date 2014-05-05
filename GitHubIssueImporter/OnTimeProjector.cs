using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GitHubIssueImporter
{
    public class OnTimeProjector
    {
        private String connectionString;

        public OnTimeProjector(String connectionString)
        {
            this.connectionString = connectionString;
        }

        public Defect GetDefect(Int32 id)
        {
            var defect = new Defect();

            using (var connection = new SqlConnection(connectionString))
            {
                var sql = @"
                    SELECT DefectId, Name, Description, ReplicationProcedures
                    FROM Defects
                    WHERE DefectId = @Id";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read() == false)
                            throw new ArgumentOutOfRangeException("id", id, "No defect was found with the provided id.");

                        defect.Id = Convert.ToInt32(reader["DefectId"]);
                        defect.Title = Convert.ToString(reader["Name"]);
                        defect.Description =  Sanitize(Convert.ToString(reader["Description"]));
                        defect.ReproductionSteps = Sanitize(Convert.ToString(reader["ReplicationProcedures"]));
                    }
                }

                defect.Comments = GetComments(id, ItemType.Defect, connection);
                defect.RelatedItems = GetRelatedItems(id, connection);
            }

            return defect;
        }

        public Feature GetFeature(Int32 id)
        {
            var feature = new Feature();

            using (var connection = new SqlConnection(connectionString))
            {
                var sql = @"
                    SELECT FeatureId, Name, Description
                    FROM Features
                    WHERE FeatureId = @Id";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read() == false)
                            throw new ArgumentOutOfRangeException("id", id, "No defect was found with the provided id.");

                        feature.Id = Convert.ToInt32(reader["FeatureId"]);
                        feature.Title = Convert.ToString(reader["Name"]);
                        feature.Description = Sanitize(Convert.ToString(reader["Description"]));
                    }
                }

                feature.Comments = GetComments(id, ItemType.Feature, connection);
                feature.RelatedItems = GetRelatedItems(id, connection);
            }

            return feature;
        }

        private IEnumerable<Comment> GetComments(Int32 itemId, ItemType itemType, SqlConnection connection)
        {
            var comments = new List<Comment>();

            var sql = @"
                SELECT u.FirstName + ' ' + u.LastName AS Commenter, c.CommentText, c.CreatedDateTime
                FROM Comments c
                INNER JOIN Users u ON c.CreatedById = u.UserId
                WHERE ItemId = @ItemId AND ItemType = @ItemType";

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@ItemId", itemId);
                command.Parameters.AddWithValue("@ItemType", (Int32)itemType);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var comment = new Comment();
                        comment.Commenter = Convert.ToString(reader["Commenter"]);
                        comment.Text = Sanitize(Convert.ToString(reader["CommentText"]));
                        comment.Timestamp = Convert.ToDateTime(reader["CreatedDateTime"]);
                        comments.Add(comment);
                    }
                }
            }
            return comments;
        }

        private static IEnumerable<RelatedItem> GetRelatedItems(Int32 id, SqlConnection connection)
        {
            var relatedItems = new List<RelatedItem>();

            var sql = @"
                SELECT ChildItemId AS Id, ChildItemTypeId AS ItemType FROM ItemRelations WHERE ParentItemId = @DefectId AND ChildItemTypeId IN (0,1)
                UNION
                SELECT ParentItemId, ParentItemTypeId FROM ItemRelations WHERE ChildItemId = @DefectId AND ParentItemTypeId IN (0,1)
                UNION
                SELECT RIGHT(i.IncidentNumber, LEN(i.IncidentNumber) - 1) IncidentNumber, 3
                FROM ItemRelations r
                INNER JOIN Incidents i ON r.ChildItemId = i.IncidentId
                WHERE r.ParentItemId = @DefectId AND r.ChildItemTypeId = 3
                UNION
                SELECT RIGHT(i.IncidentNumber, LEN(i.IncidentNumber) - 1) IncidentNumber, 3
                FROM ItemRelations r
                INNER JOIN Incidents i ON r.ParentItemId = i.IncidentId
                WHERE r.ChildItemId = @DefectId AND r.ParentItemTypeId = 3";

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@DefectId", id);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var relatedItem = new RelatedItem();
                        relatedItem.Id = Convert.ToInt32(reader["Id"]);
                        relatedItem.Type = Convert.ToInt32(reader["ItemType"]);
                        relatedItems.Add(relatedItem);
                    }
                }
            }
            return relatedItems;
        }

        private String Sanitize(String str)
        {
            var output = WebUtility.HtmlDecode(str);
            output = output.Replace("<br />", Environment.NewLine);
            output = output.Replace("<br>", Environment.NewLine);
            output = output.Replace("<BR />", Environment.NewLine);
            output = output.Replace("<BR>", Environment.NewLine);
            output = Regex.Replace(output, "<[^>]*(>|$)", String.Empty);
            return output.Trim();
        }
    }
}
