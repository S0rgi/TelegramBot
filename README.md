# 📚 Telegram Bot ProLearn – Web API + Docker

Telegram-бот ProLearn — это обучающий помощник, предоставляющий материалы и тесты пользователям. Запуск проекта организован через Docker.

---

## 🚀 Быстрый старт

Если один участник команды уже поднял контейнер с базой данных — повторный импорт или создание базы **не требуется**. Остальные могут просто подключиться к запущенной среде.

### 📥 Шаги запуска

1. **Клонируйте репозиторий:**

```bash
git clone https://github.com/S0rgi/TelegramBot.git
cd TelegramBot
```
2. **Использование настоящего токена**
в файле TelegramBot/Utilites/BotClientProvider
заменить BotToken на ваш токен телеграм бота


3. **Запустите контейнеры:**

```bash
docker compose up --build
```
После запуска откроется консоль отладки Microsoft 
там будет написано 
```Бот запущен```

Если база ещё не была развёрнута, выполните следующие действия (только один раз, одним человеком!):

---

## 🗃️ Импорт дампа в базу данных (если база пустая)

### 1. Создайте базу данных:

```bash
docker compose exec postgres psql -U postgres -c "CREATE DATABASE prolearn;"
```

### 2. Импортируйте SQL-дамп:

```bash
docker exec -i $(docker compose ps -q postgres) psql -U postgres -d prolearn < Back.sql
```

### 3. Убедитесь, что таблицы созданы:

```bash
docker compose exec postgres psql -U postgres -d prolearn -c "\dt"
```

Пример ожидаемого вывода:

```
                 List of relations
 Schema |         Name          | Type  |  Owner   
--------+-----------------------+-------+----------
 public | CorrectAnswers        | table | postgres
 public | Education             | table | postgres
 public | Questions             | table | postgres
 public | TestTitles            | table | postgres
 public | UserProgresses        | table | postgres
 public | Users                 | table | postgres
 public | __EFMigrationsHistory | table | postgres
(7 rows)
```

---

## ✅ Всё готово

После запуска и (однократного) импорта дампа бот готов к работе. Подключайтесь и начинайте обучение в Telegram.

---

## ⚙️ Технологии

- .NET 8
- Telegram.Bot API
- PostgreSQL 16
- Docker + Docker Compose