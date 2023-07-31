

using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace latest.Models;

public class MContext : IdentityDbContext<CareUser, CareRole, Guid> {
    public MContext(DbContextOptions<MContext> options): base(options) {}
    
    public DbSet<CareGiver> CareGivers { get; set; }
    public DbSet<CareConsumer> CarerConsumers { get; set; }
    public DbSet<CareRequest> CareRequests { get; set; }
    public DbSet<CareGiverCareRequest> CareGiverCareRequests { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<PaymentRequest> PaymentRequests { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<CareGiverCareRequest>().HasKey(cgcr => new { cgcr.CareGiverId, cgcr.CareRequestId });
        builder.Entity<CareGiver>().HasMany(cg => cg.CareGiverCareRequests).WithOne(cgcr => cgcr.CareGiver).OnDelete(DeleteBehavior.NoAction);
        builder.Entity<CareRequest>().HasMany(cr => cr.CareGiverCareRequests).WithOne(cgcr => cgcr.CareRequest).OnDelete(DeleteBehavior.NoAction);
        builder.Entity<ChatMessage>().HasOne(cm => cm.Sender).WithMany(cu => cu.OutgoingChatMessages).OnDelete(DeleteBehavior.NoAction);
        builder.Entity<ChatMessage>().HasOne(cm => cm.Reciever).WithMany(cu => cu.IncomingChatMessages).OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Appointment>().HasOne(a => a.CareRequest).WithMany(cr => cr.Appointments).OnDelete(DeleteBehavior.NoAction);
        builder.Entity<Appointment>().HasOne(a => a.PaymentRequest).WithOne(pr => pr.Appointment).HasForeignKey<PaymentRequest>();
        builder.Entity<Appointment>().HasOne(a => a.Review).WithOne(r => r.Appointment).HasForeignKey<Review>();
        builder.Entity<CareGiver>().HasOne(cg => cg.Course).WithOne(c => c.CareGiver).HasForeignKey<Course>();
        // builder.Entity<CareGiver>().HasMany(cr => cr.CareRequests).WithMany(cr => cr.CareGivers).OnDelete()
    }
}
public class CareRole : IdentityRole<Guid> {}
// public class GenericUserRoleStore : RoleStore<CareRole, MContext, Guid>
// {
//     public GenericUserRoleStore(MContext context, IdentityErrorDescriber? describer = null) : base(context, describer)
//     {
//     }
// }
// public class CareGiverUserStore : UserStore<CareGiver, CareRole, MContext, Guid>
// {
//     public CareGiverUserStore(MContext context, IdentityErrorDescriber? describer = null) : base(context, describer)
//     {
//     }
// }
// public class CareUserUserStore : UserStore<CareUser, CareRole, MContext, Guid>
// {
//     public CareUserUserStore(MContext context, IdentityErrorDescriber? describer = null) : base(context, describer)
//     {
//     }
// }
public class CareUser : IdentityUser<Guid> {
    [Required]
    public string FirstName { get; set; } = "";
    [Required]
    public string LastName { get; set; } = "";
    [Required]
    public string City { get; set; } = "";

    public byte[]? ProfileImage { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    public ICollection<ChatMessage> IncomingChatMessages { get; set; }

    public ICollection<ChatMessage> OutgoingChatMessages { get; set; }

    public ICollection<Appointment> Appointments { get; set; }

    public CareUser() {
        IncomingChatMessages = new Collection<ChatMessage>();
        OutgoingChatMessages = new Collection<ChatMessage>();
        Appointments = new Collection<Appointment>();
    }
}
// public class MContextFactory : IDesignTimeDbContextFactory<MContext> {
//     public MContext CreateDbContext(string[] args) {
//         var optionsbuilder = new DbContextOptionsBuilder<MContext>();
//         optionsbuilder.UseSqlServer("server=localhost; database=mamaacontext; user id=sa; password=Mischic32!; TrustServerCertificate=true;");
//         return new MContext(optionsbuilder.Options);
//     }
// }
//cpourse should be nullable
public class CareGiver : CareUser {

    public string Biography { get; set; } = "";

    public string StripeId { get; set; } = "";

    public Course Course { get; set; } = null!;

    public ICollection<CareGiverCareRequest> CareGiverCareRequests { get; set; }

    public ICollection<Appointment> Appointments { get; set; }

    public CareGiver() {
        CareGiverCareRequests = new Collection<CareGiverCareRequest>();
        Appointments = new Collection<Appointment>();
    }
}

 public class CareConsumer : CareUser {
    [Required]
    public string AddressOne { get; set; } = "";
    [Required]
    public string AddressTwo { get; set; } = "";
    [Required]
    public string ZipCode { get; set; } = "";
    
    public string SessionId { get; set; } = "";
}
// public class CareUserManager<TUser, TRole> where TUser : CareUser where TRole : CareRole {

//     private readonly MContext _context;
//     private readonly UserManager<TUser> _userManager;
//     private readonly RoleManager<TRole> _roleManager;

//     public CareUserManager(MContext context,
//         UserManager<TUser> userManager,
//         RoleManager<TRole> roleManager)
//     {
//         _context = context;
//         _userManager = userManager;
//         _roleManager = roleManager;
//     }
// }
// public class CareGiverManager : CareUserManager<CareGiver, CareRole>
// {
//     public CareGiverManager(MContext context, UserManager<CareGiver> userManager, RoleManager<CareRole> roleManager) : base(context, userManager, roleManager)
//     {
//     }
// }
// public class CareConsumerManager : CareUserManager<CareConsumer, CareRole>
// {
//     public CareConsumerManager(MContext context, UserManager<CareConsumer> userManager, RoleManager<CareRole> roleManager) : base(context, userManager, roleManager)
//     {
//     }
// }