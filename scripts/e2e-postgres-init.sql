DO $$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'orders') THEN
    CREATE DATABASE orders;
  END IF;

  IF NOT EXISTS (SELECT FROM pg_database WHERE datname = 'bookings') THEN
    CREATE DATABASE bookings;
  END IF;
END $$;
