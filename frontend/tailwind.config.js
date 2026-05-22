export default {
  content: ['./index.html', './src/**/*.{js,jsx}'],
  theme: {
    extend: {
      fontFamily: {
        sans: ['"Plus Jakarta Sans"', 'Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        display: ['"Plus Jakarta Sans"', 'Inter', 'ui-sans-serif', 'system-ui', 'sans-serif']
      },
      colors: {
        cream: {
          50: '#FBF8F3',
          100: '#F6F1E7',
          200: '#EFE6D2'
        },
        ink: {
          900: '#1F1B2E',
          700: '#3D3853',
          500: '#6C6883',
          300: '#A6A2BB'
        },
        lavender: {
          50: '#F4F1FE',
          100: '#E7E0FB',
          200: '#CFC2F7',
          300: '#B3A1F0',
          400: '#9684E6',
          500: '#7C6FE8',
          600: '#5F52CC',
          700: '#473E9B'
        },
        peach: {
          100: '#FFE6DA',
          200: '#FFCAB1',
          300: '#FFB199',
          500: '#F58A6E',
          700: '#B85638'
        },
        mint: {
          100: '#DCF3E8',
          200: '#B5E6CF',
          300: '#8FD8B5',
          500: '#4DBF8A',
          700: '#27855C'
        },
        sky: {
          100: '#DCEEFB',
          200: '#B7DCF6',
          300: '#8FC6EE',
          500: '#4F9DD8',
          700: '#22699C'
        },
        sun: {
          100: '#FFF1CC',
          200: '#FFE39E',
          300: '#FFD46B',
          500: '#E5A82A',
          700: '#9F7510'
        },
        brand: {
          50: '#F4F1FE',
          100: '#E7E0FB',
          500: '#7C6FE8',
          600: '#5F52CC',
          700: '#473E9B'
        }
      },
      boxShadow: {
        soft: '0 10px 30px -12px rgba(71, 62, 155, 0.18)',
        glow: '0 18px 40px -18px rgba(124, 111, 232, 0.45)',
        ring: '0 0 0 4px rgba(207, 194, 247, 0.45)'
      },
      borderRadius: {
        xl2: '1.25rem',
        '4xl': '2rem'
      },
      backgroundImage: {
        'pastel-hero': 'linear-gradient(135deg, #F4F1FE 0%, #FFE6DA 50%, #DCF3E8 100%)',
        'pastel-shell': 'radial-gradient(120% 80% at 10% 0%, #F4F1FE 0%, transparent 60%), radial-gradient(80% 60% at 90% 10%, #FFE6DA 0%, transparent 55%), #FBF8F3'
      }
    }
  },
  plugins: []
};
