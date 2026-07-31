using KutubxonaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<BookPage> BookPages { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<SaleBook> SaleBooks { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Email manzili unique bo'lishi shart
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // YANGI - SaleBook narxi uchun aniqlik
        modelBuilder.Entity<SaleBook>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.PriceAtOrder)
            .HasPrecision(18, 2);

        // Boshlang'ich kitoblar
        modelBuilder.Entity<Book>().HasData(
            new Book { Id = 1, Title = "O'tgan kunlar", Author = "Abdulla Qodiriy", Year = 1922, Category = "Roman", IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1) },
            new Book { Id = 2, Title = "Mehrobdan chayon", Author = "Abdulla Qodiriy", Year = 1929, Category = "Roman", IsAvailable = false, CreatedAt = new DateTime(2024, 1, 1) },
            new Book { Id = 3, Title = "Shaytanat", Author = "Tohir Malik", Year = 1996, Category = "Roman", IsAvailable = true, CreatedAt = new DateTime(2024, 1, 1) }
        );

        // Boshlang'ich sahifalar - har bir kitobga
        modelBuilder.Entity<BookPage>().HasData(
            // O'tgan kunlar (Kitob 1) - 3 ta sahifa
            new BookPage { Id = 1, BookId = 1, PageNumber = 1, Content = "Yigirma sakkizinchi yilning yanvar oyining yigirma to'qqizinchi kuni edi. Bu kun ham, har kungidek, Toshkent kuchli sovuq ostida edi. Ko'cha bo'ylab tushgan qor butun shahar yuzini oqartirib turardi. Otabek o'z xonasida uxlamasdan o'tirardi. Uning xayollari uzoq, yiroq joylarga uchayotgan edi. Onasining ovozi uni xayoldan uyg'otdi." },
            new BookPage { Id = 2, BookId = 1, PageNumber = 2, Content = "Otabek choyni iching, sovub qolyapti, deb chaqirdi onasi. Otabek o'rnidan turdi va xonadan chiqdi. Mehmonxonada otasi Yusufbek hoji o'tirardi. Otabek otasi bilan salomlashdi va dasturxon yoniga o'tirdi. Yusufbek hoji o'g'liga qarab dedi: O'g'lim, men senga bir narsa aytmoqchiman. Toshkentda bir yaxshi qiz bor. Uning ismi Kumush. Marg'ilonlik." },
            new BookPage { Id = 3, BookId = 1, PageNumber = 3, Content = "Otabek bu so'zlardan keyin jim qoldi. U Kumushni ko'rmagan edi, lekin u haqida ko'p eshitgan edi. Kumush juda go'zal va aqlli qiz edi. Otabek o'ylab turdi va dedi: Ota, men sizning so'zingizni quloq solaman. Yusufbek hoji o'g'lining bu javobidan xursand bo'ldi. Ertaga Marg'ilonga jo'naymiz, dedi." },

            // Mehrobdan chayon (Kitob 2) - 3 ta sahifa
            new BookPage { Id = 4, BookId = 2, PageNumber = 1, Content = "Mehrobdan chayon - Abdulla Qodiriyning ikkinchi mashhur tarixiy romanidir. Roman 1928-1929-yillarda yozilgan va 1929-yilda nashr etilgan. Asar XIX asrning oxiri va XX asr boshlaridagi Buxoro va Samarqand tarixiga bag'ishlangan. Roman qahramoni Anvar - yosh, bilimli va vatanparvar yigitdir." },
            new BookPage { Id = 5, BookId = 2, PageNumber = 2, Content = "U Buxoro madrasasida tahsil olgan va o'z xalqining ozodligi uchun kurashadi. Anvarning sevgilisi Ra'no esa amir Said Olimxonning xotini bo'lishga majbur etilgan ayoldir. Asarda Buxoro amirligining oxirgi yillaridagi vaziyat, xalq hayoti, mardlik va xiyonat, sevgi va ayriliq tasvirlanadi." },
            new BookPage { Id = 6, BookId = 2, PageNumber = 3, Content = "Qodiriy bu romanda ham o'zining nozik ruhshunoslik mahoratini namoyish etgan. Har bir personajning ichki kechinmalari, fikrlari va his-tuyg'ulari mohirona ko'rsatilgan. Anvar va Ra'no o'rtasidagi sevgi - sof, samimiy va fojiali. Mehrobdan chayon - faqat bir sevgi hikoyasi emas, balki butun bir davrning hikoyasidir." },

            // Shaytanat (Kitob 3) - 3 ta sahifa
            new BookPage { Id = 7, BookId = 3, PageNumber = 1, Content = "Shaytanat - Tohir Malikning eng mashhur detektiv romanidir. Roman 1996-yilda nashr etilgan. Asar O'zbekistondagi mustaqillik yillaridagi jinoyat dunyosi haqida hikoya qiladi. Bosh qahramon - tergovchi Maxsud Asadov. U o'zining tajribasi va aql-zakovati bilan eng murakkab jinoyatlarni ochib boradi." },
            new BookPage { Id = 8, BookId = 3, PageNumber = 2, Content = "Romanda qanday qilib jinoyatchilar guruhlari paydo bo'lganligi, ular qanday ishlaganligi va qanday qilib qonun himoyachilari ular bilan kurashganligi tasvirlanadi. Asar voqealari Toshkent va boshqa shaharlarda kechadi. Maxsud o'z hamkasblari bilan birga jinoyatchilarni ushlash uchun xavfli operatsiyalarda qatnashadi." },
            new BookPage { Id = 9, BookId = 3, PageNumber = 3, Content = "Birinchi bobda Maxsud ofisida o'tirib, yangi ishni ko'rib chiqyapti. Yosh tadbirkor o'ldirilgan. Hech qanday guvoh yo'q, hech qanday iz qoldirilmagan. Bu juda professional ish edi. Maxsud o'z taxminlari bilan ishlay boshladi. Asar oxirida Maxsud jinoyatchini topadi. Bu roman shunchaki detektiv emas, balki hayot haqidagi falsafiy asardir." }
        );
    }
}