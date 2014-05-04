using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GitHubIssueImporter
{
    public class OnTimeDefectProjector
    {
        private String connectionString;

        public OnTimeDefectProjector(String connectionString)
        {
            this.connectionString = connectionString;
        }

        public Defect Get(Int32 id)
        {
            var defect = new Defect();

            using (var connection = new SqlConnection(connectionString))
            {
                var sql = "SELECT * FROM Defects WHERE DefectId = @DefectId";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@DefectId", id);

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

                var comments = new List<Comment>();

                sql = @"
                    SELECT u.FirstName + ' ' + u.LastName AS Commenter, c.CommentText, c.CreatedDateTime
                    FROM Comments c
                    INNER JOIN Users u ON c.CreatedById = u.UserId
                    WHERE ItemId = @DefectId AND ItemType = 0";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@DefectId", id);

                    using (var reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            var comment = new Comment();
                            comment.Commenter = Convert.ToString(reader["Commenter"]);
                            comment.Text = Sanitize(Convert.ToString(reader["CommentText"]));
                            comment.Timestamp = Convert.ToDateTime(reader["CreatedDateTime"]);
                            comments.Add(comment);
                        }
                    }
                }

                defect.Comments = comments;

                var relatedItems = new List<RelatedItem>();

                sql = @"
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

                defect.RelatedItems = relatedItems;
            }

            return defect;
        }

        private String Sanitize(String str)
        {
            return WebUtility.HtmlDecode(str)
                .Replace("<br />", Environment.NewLine)
                .Replace("<BR />", Environment.NewLine);
        }
    }
}
