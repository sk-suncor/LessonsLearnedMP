using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Suncor.LessonsLearnedMP.Data
{
    [Serializable]
    public partial class LessonsLearnedMPEntities : DbContext
    {
        public LessonsLearnedMPEntities()
        {
        }

        public LessonsLearnedMPEntities(DbContextOptions<LessonsLearnedMPEntities> options)
            : base(options)
        {
        }

        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<Lesson> Lessons { get; set; }
        public virtual DbSet<LessonHistory> LessonHistories { get; set; }
        public virtual DbSet<LessonTheme> LessonThemes { get; set; }
        public virtual DbSet<ReferenceType> ReferenceTypes { get; set; }
        public virtual DbSet<ReferenceValue> ReferenceValues { get; set; }
        public virtual DbSet<RoleUser> RoleUsers { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer("Server=SQLDEVCGY015V2\\SQLDEVCGY015V2;Database=CLLPDEV;User Id=llp_admin;Password=pw4Llp;multipleactiveresultsets=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasMaxLength(4096)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Enabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.HasOne(d => d.CommentType)
                    .WithMany(p => p.Comment)
                    .HasForeignKey(d => d.CommentTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comment_CommentTypeReferenceValue");

                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(d => d.LessonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comment_Lesson");
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.Property(e => e.Benefit)
                    .HasMaxLength(4096)
                    .IsUnicode(false);

                entity.Property(e => e.CasualFactors)
                    .HasMaxLength(4096)
                    .IsUnicode(false);

                entity.Property(e => e.ContactEmail)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContactFirstName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContactLastName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPhone)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Coordinator)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.CoordinatorOwnerSid)
                    .HasMaxLength(184)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(4096)
                    .IsUnicode(false);

                entity.Property(e => e.Enabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.EstimatedCompletion).HasColumnType("datetime");

                entity.Property(e => e.OwnerSid)
                    .HasMaxLength(184)
                    .IsUnicode(false);

                entity.Property(e => e.Resolution)
                    .HasMaxLength(4096)
                    .IsUnicode(false);

                entity.Property(e => e.SessionDate).HasColumnType("datetime");

                entity.Property(e => e.SuggestedAction)
                    .HasMaxLength(4096)
                    .IsUnicode(false);

                entity.Property(e => e.SupportingDocuments)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ThemeDescription)
                    .HasMaxLength(4096)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.UpdateUser)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Classification)
                    .WithMany(p => p.LessonClassification)
                    .HasForeignKey(d => d.ClassificationId)
                    .HasConstraintName("FK_Lesson_ClassificationReferenceValue");

                entity.HasOne(d => d.CostImpact)
                    .WithMany(p => p.LessonCostImpact)
                    .HasForeignKey(d => d.CostImpactId)
                    .HasConstraintName("FK_Lesson_CostImpactReferenceValue");

                entity.HasOne(d => d.CredibilityChecklist)
                    .WithMany(p => p.LessonCredibilityChecklist)
                    .HasForeignKey(d => d.CredibilityChecklistId)
                    .HasConstraintName("FK_Lesson_CredibilityChecklistReferenceValue");

                entity.HasOne(d => d.Discipline)
                    .WithMany(p => p.LessonDiscipline)
                    .HasForeignKey(d => d.DisciplineId)
                    .HasConstraintName("FK_Lesson_DisciplineReferenceValue");

                entity.HasOne(d => d.ImpactBenefitRange)
                    .WithMany(p => p.LessonImpactBenefitRange)
                    .HasForeignKey(d => d.ImpactBenefitRangeId)
                    .HasConstraintName("FK_Lesson_ImpactBenefitRangeReferenceValue");

                entity.HasOne(d => d.LessonTypeInvalid)
                    .WithMany(p => p.LessonLessonTypeInvalid)
                    .HasForeignKey(d => d.LessonTypeInvalidId)
                    .HasConstraintName("FK_Lesson_LessonTypeInvalidReferenceValue");

                entity.HasOne(d => d.LessonTypeValid)
                    .WithMany(p => p.LessonLessonTypeValid)
                    .HasForeignKey(d => d.LessonTypeValidId)
                    .HasConstraintName("FK_Lesson_LessonTypeValidReferenceValue");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.LessonLocation)
                    .HasForeignKey(d => d.LocationId)
                    .HasConstraintName("FK_Lesson_LocationReferenceValue");

                entity.HasOne(d => d.Phase)
                    .WithMany(p => p.LessonPhase)
                    .HasForeignKey(d => d.PhaseId)
                    .HasConstraintName("FK_Lesson_PhaseReferenceValue");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.LessonProject)
                    .HasForeignKey(d => d.ProjectId)
                    .HasConstraintName("FK_Lesson_ProjectReferenceValue");

                entity.HasOne(d => d.RiskRanking)
                    .WithMany(p => p.LessonRiskRanking)
                    .HasForeignKey(d => d.RiskRankingId)
                    .HasConstraintName("FK_Lesson_RiskRankingReferenceValue");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.LessonStatus)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Lesson_StatusReferenceValue");
            });

            modelBuilder.Entity<LessonHistory>(entity =>
            {
                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.LessonHistories)
                    .HasForeignKey(d => d.LessonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LessonHistory_Lesson");

                entity.HasOne(d => d.NewDiscipline)
                    .WithMany(p => p.LessonHistoryNewDiscipline)
                    .HasForeignKey(d => d.NewDisciplineId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LessonHistory_NewDisciplineReferenceValue");

                entity.HasOne(d => d.NewStatus)
                    .WithMany(p => p.LessonHistoryNewStatus)
                    .HasForeignKey(d => d.NewStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LessonHistory_NewStatusReferenceValue");

                entity.HasOne(d => d.PreviousDiscipline)
                    .WithMany(p => p.LessonHistoryPreviousDiscipline)
                    .HasForeignKey(d => d.PreviousDisciplineId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LessonHistory_PreviousDisciplineReferenceValue");

                entity.HasOne(d => d.PreviousStatus)
                    .WithMany(p => p.LessonHistoryPreviousStatus)
                    .HasForeignKey(d => d.PreviousStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LessonHistory_PreviousStatusReferenceValue");
            });

            modelBuilder.Entity<LessonTheme>(entity =>
            {
                entity.HasOne(d => d.Lesson)
                    .WithMany(p => p.LessonThemes)
                    .HasForeignKey(d => d.LessonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LessonTheme_Lesson");

                entity.HasOne(d => d.Theme)
                    .WithMany(p => p.LessonTheme)
                    .HasForeignKey(d => d.ThemeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_LessonTheme_ThemeReferenceValue");
            });

            modelBuilder.Entity<ReferenceType>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ReferenceValue>(entity =>
            {
                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Enabled)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.SortOrder).HasDefaultValueSql("((1))");

                entity.Property(e => e.UpdateDate).HasColumnType("datetime");

                entity.Property(e => e.UpdateUser)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.HasOne(d => d.ReferenceType)
                    .WithMany(p => p.ReferenceValue)
                    .HasForeignKey(d => d.ReferenceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReferenceValue_ReferenceType");
            });

            modelBuilder.Entity<RoleUser>(entity =>
            {
                entity.HasKey(e => e.Sid);

                entity.Property(e => e.Sid)
                    .HasMaxLength(184)
                    .IsUnicode(false);

                entity.Property(e => e.AutoUpdate)
                    .IsRequired()
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.CreateUser)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.DistinguishedName)
                    .HasMaxLength(1024)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Discipline)
                    .WithMany(p => p.RoleUserDiscipline)
                    .HasForeignKey(d => d.DisciplineId)
                    .HasConstraintName("FK_RoleUser_DisciplineReferenceValue");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RoleUserRole)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RoleUser_RoleReferenceValue");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
