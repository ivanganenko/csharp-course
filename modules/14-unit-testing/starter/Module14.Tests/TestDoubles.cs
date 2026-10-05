// Задача 14.3 — здесь напиши свои test doubles (без библиотек):
//
// 1. FakeClock : IClock
//    - конструктор принимает DateTimeOffset «сейчас»;
//    - свойство UtcNow можно менять (или метод Advance(TimeSpan)), чтобы «перемотать время» в тесте.
//
// 2. FakeEmailSender : IEmailSender
//    - не отправляет письма, а складывает их в список (например, IReadOnlyList<SentEmail> Sent);
//    - SentEmail — record с To, Subject, Body.
//
// Не забудь: namespace Module14.Tests; и using Module14.Tasks;
