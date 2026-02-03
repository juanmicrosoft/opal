import type { Metadata } from 'next';
import { Inter, JetBrains_Mono, VT323 } from 'next/font/google';
import { Header } from '@/components/Header';
import { Footer } from '@/components/Footer';
import './globals.css';

const inter = Inter({
  subsets: ['latin'],
  variable: '--font-inter',
});

const jetbrainsMono = JetBrains_Mono({
  subsets: ['latin'],
  variable: '--font-mono',
});

const vt323 = VT323({
  weight: '400',
  subsets: ['latin'],
  variable: '--font-terminal',
});

export const metadata: Metadata = {
  title: {
    default: 'Calor - Coding Agent Language for Optimized Reasoning',
    template: '%s | Calor',
  },
  description:
    'A programming language designed specifically for AI coding agents, compiling to .NET via C# emission.',
  keywords: ['Calor', 'programming language', 'AI', 'coding agents', 'compiler', '.NET', 'C#'],
  authors: [{ name: 'Calor Team' }],
  openGraph: {
    type: 'website',
    locale: 'en_US',
    url: 'https://juanmicrosoft.github.io/calor',
    siteName: 'Calor',
    title: 'Calor - Coding Agent Language for Optimized Reasoning',
    description:
      'A programming language designed specifically for AI coding agents, compiling to .NET via C# emission.',
  },
  twitter: {
    card: 'summary_large_image',
    title: 'Calor - Coding Agent Language for Optimized Reasoning',
    description:
      'A programming language designed specifically for AI coding agents, compiling to .NET via C# emission.',
  },
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={`${inter.variable} ${jetbrainsMono.variable} ${vt323.variable} font-sans antialiased`}>
        <div className="relative flex min-h-screen flex-col">
          <Header />
          <main className="flex-1">{children}</main>
          <Footer />
        </div>
      </body>
    </html>
  );
}
