using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Oblig3.Models;

public partial class Dat154Context : DbContext
{
    public Dat154Context()
    {
    }

    public Dat154Context(DbContextOptions<Dat154Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("data source=dat154demo.database.windows.net;Initial Catalog=dat154;User ID=dat154_rw;Password=Svart_Katt;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Danish_Norwegian_CI_AS");

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Coursecode).HasName("pk_course");

            entity.ToTable("course");

            entity.Property(e => e.Coursecode)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("coursecode");
            entity.Property(e => e.Coursename)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("coursename");
            entity.Property(e => e.Semester)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("semester");
            entity.Property(e => e.Teacher)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("teacher");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => new { e.Coursecode, e.Studentid }).HasName("pk_grade");

            entity.ToTable("grade");

            entity.Property(e => e.Coursecode)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("coursecode");
            entity.Property(e => e.Studentid).HasColumnName("studentid");
            entity.Property(e => e.Grade1)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("grade");

            entity.HasOne(d => d.CoursecodeNavigation).WithMany(p => p.Grades)
                .HasForeignKey(d => d.Coursecode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_course");

            entity.HasOne(d => d.Student).WithMany(p => p.Grades)
                .HasForeignKey(d => d.Studentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_student");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Primary");

            entity.ToTable("student");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Studentage).HasColumnName("studentage");
            entity.Property(e => e.Studentname)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("studentname");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    public async Task<List<Student>> SearchStudent(string student) {
        return await Students.Where(s => s.Studentname.Contains(student)).ToListAsync();
    }

    public async Task<List<Course>> SearchCourse(string course) {
        return await Courses.Where(s => s.Coursename.Contains(course)).ToListAsync();
    }

    public async Task<List<StudentGrade>> InCourse(string course) {
        return await Grades
            .Include(c => c.Student)
            .Where(c => c.Coursecode == course)
            .Select(c => new StudentGrade { Student = c.Student, Grade = c})
            .ToListAsync();
    }

    public async Task<List<StudentGradeCourse>> AboveGrade(string grade) {
        return await Grades
            .Include(g => g.Student)
            .Include(g => g.Course)
            .Where(g => string.Compare(g.Grade1, grade) <= 0)
            .Select(g => new StudentGradeCourse { Student = g.Student, Grade = g, Course = g.Course })
            .ToListAsync();
    }
}
