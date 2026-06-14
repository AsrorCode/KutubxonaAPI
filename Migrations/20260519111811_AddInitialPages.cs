using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KutubxonaAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BookPages",
                columns: new[] { "Id", "BookId", "Content", "PageNumber" },
                values: new object[,]
                {
                    { 1, 1, "Yigirma sakkizinchi yilning yanvar oyining yigirma to'qqizinchi kuni edi. Bu kun ham, har kungidek, Toshkent kuchli sovuq ostida edi. Ko'cha bo'ylab tushgan qor butun shahar yuzini oqartirib turardi. Otabek o'z xonasida uxlamasdan o'tirardi. Uning xayollari uzoq, yiroq joylarga uchayotgan edi. Onasining ovozi uni xayoldan uyg'otdi.", 1 },
                    { 2, 1, "Otabek choyni iching, sovub qolyapti, deb chaqirdi onasi. Otabek o'rnidan turdi va xonadan chiqdi. Mehmonxonada otasi Yusufbek hoji o'tirardi. Otabek otasi bilan salomlashdi va dasturxon yoniga o'tirdi. Yusufbek hoji o'g'liga qarab dedi: O'g'lim, men senga bir narsa aytmoqchiman. Toshkentda bir yaxshi qiz bor. Uning ismi Kumush. Marg'ilonlik.", 2 },
                    { 3, 1, "Otabek bu so'zlardan keyin jim qoldi. U Kumushni ko'rmagan edi, lekin u haqida ko'p eshitgan edi. Kumush juda go'zal va aqlli qiz edi. Otabek o'ylab turdi va dedi: Ota, men sizning so'zingizni quloq solaman. Yusufbek hoji o'g'lining bu javobidan xursand bo'ldi. Ertaga Marg'ilonga jo'naymiz, dedi.", 3 },
                    { 4, 2, "Mehrobdan chayon - Abdulla Qodiriyning ikkinchi mashhur tarixiy romanidir. Roman 1928-1929-yillarda yozilgan va 1929-yilda nashr etilgan. Asar XIX asrning oxiri va XX asr boshlaridagi Buxoro va Samarqand tarixiga bag'ishlangan. Roman qahramoni Anvar - yosh, bilimli va vatanparvar yigitdir.", 1 },
                    { 5, 2, "U Buxoro madrasasida tahsil olgan va o'z xalqining ozodligi uchun kurashadi. Anvarning sevgilisi Ra'no esa amir Said Olimxonning xotini bo'lishga majbur etilgan ayoldir. Asarda Buxoro amirligining oxirgi yillaridagi vaziyat, xalq hayoti, mardlik va xiyonat, sevgi va ayriliq tasvirlanadi.", 2 },
                    { 6, 2, "Qodiriy bu romanda ham o'zining nozik ruhshunoslik mahoratini namoyish etgan. Har bir personajning ichki kechinmalari, fikrlari va his-tuyg'ulari mohirona ko'rsatilgan. Anvar va Ra'no o'rtasidagi sevgi - sof, samimiy va fojiali. Mehrobdan chayon - faqat bir sevgi hikoyasi emas, balki butun bir davrning hikoyasidir.", 3 },
                    { 7, 3, "Shaytanat - Tohir Malikning eng mashhur detektiv romanidir. Roman 1996-yilda nashr etilgan. Asar O'zbekistondagi mustaqillik yillaridagi jinoyat dunyosi haqida hikoya qiladi. Bosh qahramon - tergovchi Maxsud Asadov. U o'zining tajribasi va aql-zakovati bilan eng murakkab jinoyatlarni ochib boradi.", 1 },
                    { 8, 3, "Romanda qanday qilib jinoyatchilar guruhlari paydo bo'lganligi, ular qanday ishlaganligi va qanday qilib qonun himoyachilari ular bilan kurashganligi tasvirlanadi. Asar voqealari Toshkent va boshqa shaharlarda kechadi. Maxsud o'z hamkasblari bilan birga jinoyatchilarni ushlash uchun xavfli operatsiyalarda qatnashadi.", 2 },
                    { 9, 3, "Birinchi bobda Maxsud ofisida o'tirib, yangi ishni ko'rib chiqyapti. Yosh tadbirkor o'ldirilgan. Hech qanday guvoh yo'q, hech qanday iz qoldirilmagan. Bu juda professional ish edi. Maxsud o'z taxminlari bilan ishlay boshladi. Asar oxirida Maxsud jinoyatchini topadi. Bu roman shunchaki detektiv emas, balki hayot haqidagi falsafiy asardir.", 3 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BookPages",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BookPages",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BookPages",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "BookPages",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BookPages",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "BookPages",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "BookPages",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "BookPages",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "BookPages",
                keyColumn: "Id",
                keyValue: 9);
        }
    }
}
