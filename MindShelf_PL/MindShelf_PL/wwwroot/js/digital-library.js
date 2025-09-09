/**
 * MindShelf Digital Library JavaScript
 * البحث الذكي، الوضع الليلي، والتفاعلات
 */

// ========== Global Variables ==========
let searchTimeout;
let isDarkMode = localStorage.getItem('darkMode') === 'true';

// ========== Document Ready ==========
document.addEventListener('DOMContentLoaded', function () {
    initializeComponents();
    initializeSearchToggle();
});

// ========== Initialization ==========
function initializeComponents() {
    initDarkMode();
    initSmartSearch();
    initScrollEffects();
    initAnimations();
    initLoadingOverlay();
    initTooltips();
    initFormValidation();
    preloadBooks(); // Preload books for instant search
}

// ========== Dark Mode ==========
function initDarkMode() {
    const darkModeToggle = document.getElementById('darkModeToggle');
    const body = document.body;

    // تطبيق الوضع المحفوظ
    if (isDarkMode) {
        body.classList.add('dark-mode');
        updateDarkModeIcon(true);
    }

    // إضافة مستمع للنقر
    if (darkModeToggle) {
        darkModeToggle.addEventListener('click', function () {
            isDarkMode = !isDarkMode;

            if (isDarkMode) {
                body.classList.add('dark-mode');
                localStorage.setItem('darkMode', 'true');
                showNotification('تم التبديل للوضع الليلي', 'success');
            } else {
                body.classList.remove('dark-mode');
                localStorage.setItem('darkMode', 'false');
                showNotification('تم التبديل للوضع النهاري', 'success');
            }

            updateDarkModeIcon(isDarkMode);
        });
    }
}

function updateDarkModeIcon(isDark) {
    const icon = document.querySelector('#darkModeToggle i');
    if (icon) {
        icon.className = isDark ? 'fas fa-sun' : 'fas fa-moon';
    }
}

// ========== Smart Search ==========
function initSmartSearch() {
    const searchInput = document.getElementById('smartSearch');
    const searchSuggestions = document.getElementById('searchSuggestions');

    console.log('Initializing smart search...');
    console.log('Search input found:', !!searchInput);
    console.log('Search suggestions found:', !!searchSuggestions);

    if (!searchInput || !searchSuggestions) {
        console.error('Search elements not found!');
        return;
    }

    searchInput.addEventListener('input', function () {
        const query = this.value.trim();
        console.log('Input event triggered, query:', query);

        // مسح المؤقت السابق
        clearTimeout(searchTimeout);

        if (query.length < 1) {
            hideSuggestions();
            return;
        }

        // بحث فوري بدون تأخير
        console.log('Performing search for:', query);
        performSearch(query);
    });

    // إخفاء الاقتراحات عند النقر خارجها
    document.addEventListener('click', function (e) {
        if (!searchInput.contains(e.target) && !searchSuggestions.contains(e.target)) {
            hideSuggestions();
        }
    });

    // التنقل بالكيبورد
    searchInput.addEventListener('keydown', function (e) {
        handleSearchKeyNavigation(e);
    });
}

async function performSearch(query) {
    const searchSuggestions = document.getElementById('searchSuggestions');
    console.log('performSearch called with query:', query);

    try {
        // عرض مؤشر التحميل
        searchSuggestions.innerHTML = '<div class="search-suggestion-item"><i class="fas fa-spinner fa-spin me-2"></i>جاري البحث...</div>';
        searchSuggestions.style.display = 'block';
        console.log('Loading indicator shown');

        // البحث الفعلي
        const suggestions = await fetchSearchSuggestions(query);
        console.log('Suggestions received:', suggestions);

        displaySuggestions(suggestions);

    } catch (error) {
        console.error('خطأ في البحث:', error);
        searchSuggestions.innerHTML = '<div class="search-suggestion-item text-danger"><i class="fas fa-exclamation-triangle me-2"></i>حدث خطأ في البحث</div>';
        searchSuggestions.style.display = 'block';
    }
}

// Cache for all books to enable client-side filtering
let allBooksCache = [];
let cacheLoaded = false;

// Preload books for instant search
async function preloadBooks() {
    try {
        await loadAllBooks();
        console.log('Books preloaded successfully');
    } catch (error) {
        console.error('Error preloading books:', error);
    }
}

async function fetchSearchSuggestions(query) {
    try {
        console.log('Searching for:', query);
        
        // Load all books if not cached
        if (!cacheLoaded) {
            console.log('Cache not loaded, loading books...');
            await loadAllBooks();
        }
        
        // If cache is still empty, try direct search
        if (allBooksCache.length === 0) {
            console.log('Cache is empty, trying direct search...');
            return await directSearch(query);
        }
        
        // Filter books client-side for instant results
        const filteredBooks = allBooksCache.filter(book => 
            book.title.toLowerCase().includes(query.toLowerCase()) ||
            book.author.toLowerCase().includes(query.toLowerCase())
        );
        
        console.log(`Found ${filteredBooks.length} books matching "${query}"`);
        return filteredBooks.slice(0, 10); // Show up to 10 results
        
    } catch (error) {
        console.error('Error fetching search suggestions:', error);
        return [];
    }
}

// Fallback direct search function
async function directSearch(query) {
    try {
        console.log('Performing direct search for:', query);
        
        const response = await fetch(`/Books/Search?searchTerm=${encodeURIComponent(query)}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'text/html',
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const html = await response.text();
        
        // Parse the HTML response to extract book data
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const bookCards = doc.querySelectorAll('.book-card');
        
        console.log('Direct search found book cards:', bookCards.length);
        
        const suggestions = [];
        
        bookCards.forEach((card, index) => {
            console.log(`Processing card ${index}:`, card);
            
            // Try multiple selectors for title
            const titleElement = card.querySelector('.card-title, h5, h6, .fw-bold');
            const authorElement = card.querySelector('.text-muted, small');
            const imageElement = card.querySelector('img');
            const priceElement = card.querySelector('.h6.text-success.fw-bold, .text-success, .price');
            
            // Try multiple selectors for link or use data-book-id
            const linkElement = card.querySelector('a[href*="/Books/Details/"], a[href*="/Books/Details"], a[onclick*="viewBookDetails"]');
            const bookIdFromData = card.getAttribute('data-book-id');
            
            console.log(`Card ${index} elements:`, {
                titleElement: titleElement?.textContent?.trim(),
                authorElement: authorElement?.textContent?.trim(),
                imageElement: imageElement?.src,
                priceElement: priceElement?.textContent?.trim(),
                priceElementClass: priceElement?.className,
                linkElement: linkElement?.href,
                bookIdFromData: bookIdFromData
            });
            
            if (titleElement) {
                const title = titleElement.textContent.trim();
                const author = authorElement ? authorElement.textContent.trim() : 'غير محدد';
                const imageUrl = imageElement ? imageElement.src : null;
                
                // Better price extraction - look for price patterns
                let price = null;
                if (priceElement) {
                    const priceText = priceElement.textContent.trim();
                    // Check if it looks like a price (contains numbers and currency)
                    if (priceText.match(/\d+/) && (priceText.includes('ر.س') || priceText.includes('جنيه') || priceText.includes('$') || priceText.includes('£') || priceText.includes('مجاناً'))) {
                        price = priceText;
                    }
                }
                
                // If no price found, try to find it in other elements
                if (!price) {
                    const allTextElements = card.querySelectorAll('*');
                    for (let elem of allTextElements) {
                        const text = elem.textContent.trim();
                        if (text.match(/\d+/) && (text.includes('ر.س') || text.includes('جنيه') || text.includes('$') || text.includes('£') || text.includes('مجاناً'))) {
                            price = text;
                            break;
                        }
                    }
                }
                
                // Get book ID from either link or data attribute
                let bookId = null;
                if (linkElement) {
                    const href = linkElement.getAttribute('href');
                    bookId = href.match(/\/Books\/Details\/(\d+)/)?.[1];
                } else if (bookIdFromData) {
                    bookId = bookIdFromData;
                }
                
                console.log(`Book ${index}:`, { title, author, imageUrl, price, bookId });
                
                if (bookId) {
                    suggestions.push({
                        type: 'book',
                        title: title,
                        author: author,
                        imageUrl: imageUrl,
                        price: price,
                        id: parseInt(bookId)
                    });
                } else {
                    console.log(`Card ${index} missing book ID`);
                }
            } else {
                console.log(`Card ${index} missing title element`);
            }
        });

        console.log('Direct search suggestions:', suggestions.length);
        return suggestions.slice(0, 10);
        
    } catch (error) {
        console.error('Error in direct search:', error);
        return [];
    }
}

async function loadAllBooks() {
    try {
        console.log('Loading all books...');
        
        const response = await fetch('/Books/Index', {
            method: 'GET',
            headers: {
                'Content-Type': 'text/html',
            }
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const html = await response.text();
        
        // Parse the HTML response to extract all book data
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const bookCards = doc.querySelectorAll('.book-card');
        
        console.log('Found book cards:', bookCards.length);
        
        allBooksCache = [];
        
        bookCards.forEach((card, index) => {
            const titleElement = card.querySelector('.card-title, h5, h6');
            const authorElement = card.querySelector('.text-muted');
            const imageElement = card.querySelector('img');
            const priceElement = card.querySelector('.h6.text-success.fw-bold, .text-success, .price');
            const linkElement = card.querySelector('a[href*="/Books/Details/"]');
            
            if (titleElement && linkElement) {
                const title = titleElement.textContent.trim();
                const author = authorElement ? authorElement.textContent.trim() : 'غير محدد';
                const imageUrl = imageElement ? imageElement.src : null;
                
                // Better price extraction - look for price patterns
                let price = null;
                if (priceElement) {
                    const priceText = priceElement.textContent.trim();
                    // Check if it looks like a price (contains numbers and currency)
                    if (priceText.match(/\d+/) && (priceText.includes('ر.س') || priceText.includes('جنيه') || priceText.includes('$') || priceText.includes('£') || priceText.includes('مجاناً'))) {
                        price = priceText;
                    }
                }
                
                // If no price found, try to find it in other elements
                if (!price) {
                    const allTextElements = card.querySelectorAll('*');
                    for (let elem of allTextElements) {
                        const text = elem.textContent.trim();
                        if (text.match(/\d+/) && (text.includes('ر.س') || text.includes('جنيه') || text.includes('$') || text.includes('£') || text.includes('مجاناً'))) {
                            price = text;
                            break;
                        }
                    }
                }
                
                const href = linkElement.getAttribute('href');
                const bookId = href.match(/\/Books\/Details\/(\d+)/)?.[1];
                
                if (bookId) {
                    allBooksCache.push({
                        type: 'book',
                        title: title,
                        author: author,
                        imageUrl: imageUrl,
                        price: price,
                        id: parseInt(bookId)
                    });
                }
            }
        });

        cacheLoaded = true;
        console.log(`Loaded ${allBooksCache.length} books into cache`);
        
    } catch (error) {
        console.error('Error loading all books:', error);
        cacheLoaded = false;
    }
}

function displaySuggestions(suggestions) {
    const searchSuggestions = document.getElementById('searchSuggestions');

    if (suggestions.length === 0) {
        searchSuggestions.innerHTML = '<div class="search-suggestion-item text-center text-muted"><i class="fas fa-search me-2"></i>لا توجد نتائج</div>';
        searchSuggestions.style.display = 'block';
        return;
    }

    const html = suggestions.map((item, index) => {
        if (item.type === 'book') {
            const bookImage = item.imageUrl || '/Images/books/default-book.jpg';
            return `
                <div class="search-suggestion-item-full" onclick="goToBook(${item.id})" data-index="${index}">
                    <div class="search-book-image-container">
                        <img src="${bookImage}" alt="${item.title}" class="search-book-image" 
                             onerror="this.src='/Images/books/default-book.jpg'">
                    </div>
                    <div class="search-content-full">
                        <div class="search-title-full fw-bold">${item.title}</div>
                        <div class="search-author-full text-muted">
                            <i class="fas fa-user me-1"></i>${item.author}
                        </div>
                        <div class="search-price-full text-success fw-bold">
                            ${item.price && item.price.trim() ? item.price : 'السعر غير متوفر'}
                        </div>
                    </div>
                    <div class="search-arrow-full">
                        <i class="fas fa-arrow-left text-muted"></i>
                    </div>
                </div>
            `;
        } else if (item.type === 'author') {
            return `
                <div class="search-suggestion-item-full" onclick="goToAuthor(${item.id})" data-index="${index}">
                    <div class="search-icon-container-full">
                        <i class="fas fa-user-edit text-brown"></i>
                    </div>
                    <div class="search-content-full">
                        <div class="search-title-full fw-bold">${item.name}</div>
                        <div class="search-author-full text-muted">
                            <i class="fas fa-books me-1"></i>${item.booksCount} كتاب
                        </div>
                    </div>
                    <div class="search-arrow-full">
                        <i class="fas fa-arrow-left text-muted"></i>
                    </div>
                </div>
            `;
        } else if (item.type === 'category') {
            return `
                <div class="search-suggestion-item-full" onclick="goToCategory(${item.id})" data-index="${index}">
                    <div class="search-icon-container-full">
                        <i class="fas fa-list text-info"></i>
                    </div>
                    <div class="search-content-full">
                        <div class="search-title-full fw-bold">${item.name}</div>
                        <div class="search-author-full text-muted">
                            <i class="fas fa-books me-1"></i>${item.booksCount} كتاب
                        </div>
                    </div>
                    <div class="search-arrow-full">
                        <i class="fas fa-arrow-left text-muted"></i>
                    </div>
                </div>
            `;
        }
    }).join('');

    searchSuggestions.innerHTML = html;
    searchSuggestions.style.display = 'block';
}

function hideSuggestions() {
    const searchSuggestions = document.getElementById('searchSuggestions');
    if (searchSuggestions) {
        searchSuggestions.style.display = 'none';
    }
}

function handleSearchKeyNavigation(e) {
    const suggestions = document.querySelectorAll('.search-suggestion-item');
    let currentIndex = Array.from(suggestions).findIndex(item => item.classList.contains('active'));

    switch (e.key) {
        case 'ArrowDown':
            e.preventDefault();
            currentIndex = (currentIndex + 1) % suggestions.length;
            updateActiveSuggestion(suggestions, currentIndex);
            break;

        case 'ArrowUp':
            e.preventDefault();
            currentIndex = currentIndex <= 0 ? suggestions.length - 1 : currentIndex - 1;
            updateActiveSuggestion(suggestions, currentIndex);
            break;

        case 'Enter':
            e.preventDefault();
            if (currentIndex >= 0) {
                suggestions[currentIndex].click();
            }
            break;

        case 'Escape':
            hideSuggestions();
            break;
    }
}

function updateActiveSuggestion(suggestions, activeIndex) {
    suggestions.forEach((item, index) => {
        if (index === activeIndex) {
            item.classList.add('active');
            item.style.backgroundColor = 'var(--light-beige)';
        } else {
            item.classList.remove('active');
            item.style.backgroundColor = '';
        }
    });
}

// ========== Navigation Functions ==========
function goToBook(bookId) {
    window.location.href = `/Books/Details/${bookId}`;
}

function goToAuthor(authorId) {
    window.location.href = `/Author/Details/${authorId}`;
}

function goToCategory(categoryId) {
    window.location.href = `/Books/ByCategory/${categoryId}`;
}

// ========== Scroll Effects ==========
function initScrollEffects() {
    const header = document.querySelector('.digital-header');

    window.addEventListener('scroll', function () {
        if (window.scrollY > 100) {
            header?.classList.add('scrolled');
        } else {
            header?.classList.remove('scrolled');
        }
    });
}

// ========== Animations ==========
function initAnimations() {
    // تحريك العناصر عند الظهور
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
            }
        });
    });

    // مراقبة العناصر
    document.querySelectorAll('.book-card, .category-card, .stat-item').forEach(el => {
        observer.observe(el);
    });
}

// ========== Loading Overlay ==========
function initLoadingOverlay() {
    // إخفاء التحميل عند اكتمال الصفحة
    window.addEventListener('load', function () {
        hideLoadingOverlay();
    });
}

function showLoadingOverlay() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.classList.add('show');
    }
}

function hideLoadingOverlay() {
    const overlay = document.getElementById('loadingOverlay');
    if (overlay) {
        overlay.classList.remove('show');
    }
}

// ========== Tooltips ==========
function initTooltips() {
    // تفعيل Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// ========== Form Validation ==========
function initFormValidation() {
    const forms = document.querySelectorAll('form');

    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
                showNotification('يرجى ملء جميع الحقول المطلوبة', 'warning');
            } else {
                showLoadingOverlay();
            }

            form.classList.add('was-validated');
        });
    });
}

// ========== Notifications ==========
function showNotification(message, type = 'info') {
    // إنشاء عنصر الإشعار المحسن
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    
    const iconMap = {
        'success': 'fas fa-check-circle',
        'warning': 'fas fa-exclamation-triangle',
        'danger': 'fas fa-times-circle',
        'info': 'fas fa-info-circle'
    };

    const typeLabels = {
        'success': 'نجح',
        'warning': 'تحذير',
        'danger': 'خطأ',
        'info': 'معلومات'
    };

    notification.innerHTML = `
        <div class="notification-content">
            <div class="notification-icon">
                <i class="${iconMap[type] || iconMap.info}"></i>
            </div>
            <div class="notification-body">
                <div class="notification-title">${typeLabels[type] || 'إشعار'}</div>
                <div class="notification-message">${message}</div>
            </div>
            <button type="button" class="notification-close" onclick="this.parentElement.parentElement.remove()">
                <i class="fas fa-times"></i>
            </button>
        </div>
        <div class="notification-progress"></div>
    `;

    document.body.appendChild(notification);

    // إضافة تأثير الظهور
    setTimeout(() => {
        notification.classList.add('show');
    }, 100);

    // إزالة الإشعار تلقائياً مع تأثير الاختفاء
    setTimeout(() => {
        if (notification.parentNode) {
            notification.classList.add('hide');
            setTimeout(() => {
                if (notification.parentNode) {
                    notification.remove();
                }
            }, 300);
        }
    }, 4000);
}

// ========== AJAX Helpers ==========
async function fetchData(url, options = {}) {
    try {
        showLoadingOverlay();

        const response = await fetch(url, {
            headers: {
                'Content-Type': 'application/json',
                ...options.headers
            },
            ...options
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        return await response.json();

    } catch (error) {
        console.error('Fetch error:', error);
        showNotification('حدث خطأ في الاتصال بالخادم', 'danger');
        throw error;
    } finally {
        hideLoadingOverlay();
    }
}

// ========== Book Card Actions ==========
function addToFavorites(bookId) {
    fetchData('/FavouriteBook/Add', {
        method: 'POST',
        body: JSON.stringify({ bookId: bookId })
    }).then(() => {
        showNotification('تم إضافة الكتاب للمفضلة', 'success');
        // تحديث أيقونة القلب
        const heartIcon = document.querySelector(`[data-book-id="${bookId}"] .favorite-btn i`);
        if (heartIcon) {
            heartIcon.classList.remove('far');
            heartIcon.classList.add('fas');
            heartIcon.style.color = '#dc3545';
        }
    }).catch(() => {
        showNotification('فشل في إضافة الكتاب للمفضلة', 'danger');
    });
}

function addToCart(bookId) {
    fetchData('/Cart/AddToCart', {
        method: 'POST',
        body: JSON.stringify({ bookId: bookId, quantity: 1 })
    }).then(() => {
        showNotification('تم إضافة الكتاب للسلة', 'success');
        updateCartCounter();
    }).catch(() => {
        showNotification('فشل في إضافة الكتاب للسلة', 'danger');
    });
}

function updateCartCounter() {
    // تحديث عداد السلة في الواجهة
    const cartCounter = document.querySelector('.cart-counter');
    if (cartCounter) {
        const currentCount = parseInt(cartCounter.textContent) || 0;
        cartCounter.textContent = currentCount + 1;
    }
}

// ========== Rating System ==========
function initRatingSystem() {
    document.querySelectorAll('.rating-stars').forEach(ratingContainer => {
        const stars = ratingContainer.querySelectorAll('.star');
        let currentRating = 0;

        stars.forEach((star, index) => {
            star.addEventListener('mouseover', () => {
                highlightStars(stars, index + 1);
            });

            star.addEventListener('mouseout', () => {
                highlightStars(stars, currentRating);
            });

            star.addEventListener('click', () => {
                currentRating = index + 1;
                submitRating(ratingContainer.dataset.bookId, currentRating);
            });
        });
    });
}

function highlightStars(stars, rating) {
    stars.forEach((star, index) => {
        if (index < rating) {
            star.classList.add('active');
        } else {
            star.classList.remove('active');
        }
    });
}

function submitRating(bookId, rating) {
    fetchData('/Review/Rate', {
        method: 'POST',
        body: JSON.stringify({ bookId: bookId, rating: rating })
    }).then(() => {
        showNotification('تم تسجيل تقييمك بنجاح', 'success');
    }).catch(() => {
        showNotification('فشل في تسجيل التقييم', 'danger');
    });
}

// ========== Utility Functions ==========
function formatPrice(price) {
    return new Intl.NumberFormat('ar-SA', {
        style: 'currency',
        currency: 'SAR'
    }).format(price);
}

function formatDate(dateString) {
    return new Intl.DateTimeFormat('ar-SA', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    }).format(new Date(dateString));
}

// ========== Print Functions ==========
function printPage() {
    window.print();
}

function printBookDetails(bookId) {
    const printWindow = window.open(`/Books/PrintDetails/${bookId}`, '_blank');
    printWindow.onload = function () {
        printWindow.print();
        printWindow.close();
    };
}

// ========== Export Functions ==========
window.MindShelf = {
    showNotification,
    addToFavorites,
    addToCart,
    goToBook,
    goToAuthor,
    goToCategory,
    showLoadingOverlay,
    hideLoadingOverlay,
    formatPrice,
    formatDate,
    printPage,
    printBookDetails
};

// ========== Debug Mode ==========
if (window.location.hostname === 'localhost') {
    window.debug = {
        isDarkMode: () => isDarkMode,
        showAllNotifications: () => {
            showNotification('رسالة نجاح', 'success');
            setTimeout(() => showNotification('رسالة تحذير', 'warning'), 1000);
            setTimeout(() => showNotification('رسالة خطأ', 'danger'), 2000);
            setTimeout(() => showNotification('رسالة معلومات', 'info'), 3000);
        },
        testSearch: (query) => performSearch(query),
        testFetch: async (query) => {
            try {
                const response = await fetch(`/Books/Search?searchTerm=${encodeURIComponent(query)}`);
                const html = await response.text();
                console.log('Raw response:', html.substring(0, 500));
                return html;
            } catch (error) {
                console.error('Test fetch error:', error);
                return null;
            }
        },
        testBooks: () => {
            console.log('All books cache:', allBooksCache);
            console.log('Cache loaded:', cacheLoaded);
            return allBooksCache;
        },
        loadBooks: () => loadAllBooks(),
        testInput: () => {
            const input = document.getElementById('smartSearch');
            console.log('Search input element:', input);
            console.log('Search input value:', input?.value);
            return input;
        },
        inspectHTML: async (query) => {
            try {
                const response = await fetch(`/Books/Search?searchTerm=${encodeURIComponent(query)}`);
                const html = await response.text();
                console.log('Full HTML response:', html);
                
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                const bookCards = doc.querySelectorAll('.book-card');
                console.log('Book cards found:', bookCards.length);
                
                if (bookCards.length > 0) {
                    console.log('First book card HTML:', bookCards[0].outerHTML);
                }
                
                return html;
            } catch (error) {
                console.error('Error inspecting HTML:', error);
                return null;
            }
        }
    };

    console.log('🚀 MindShelf Debug Mode Active');
    console.log('Available commands:');
    console.log('- debug.testSearch("harry") - Test search');
    console.log('- debug.testBooks() - Show cached books');
    console.log('- debug.loadBooks() - Reload books');
    console.log('- debug.testInput() - Check search input');
}

// ========== Search Toggle Functionality ==========
function initializeSearchToggle() {
    const searchToggleBtn = document.getElementById('searchToggleBtn');
    const searchOverlay = document.getElementById('searchOverlay');
    const searchInput = document.getElementById('smartSearch');
    
    if (!searchToggleBtn || !searchOverlay) return;
    
    let isSearchVisible = false;
    
    searchToggleBtn.addEventListener('click', function(e) {
        e.preventDefault();
        e.stopPropagation();
        
        if (isSearchVisible) {
            // Hide search
            searchOverlay.style.display = 'none';
            searchOverlay.classList.remove('show');
            isSearchVisible = false;
            
            // Change icon to search
            searchToggleBtn.innerHTML = '<i class="fas fa-search"></i>';
            
            // Remove body scroll lock
            document.body.style.overflow = '';
        } else {
            // Show search
            searchOverlay.style.display = 'block';
            setTimeout(() => {
                searchOverlay.classList.add('show');
            }, 10);
            isSearchVisible = true;
            
            // Focus on input after animation
            setTimeout(() => {
                searchInput.focus();
            }, 400);
            
            // Change icon to close
            searchToggleBtn.innerHTML = '<i class="fas fa-times"></i>';
            
            // Lock body scroll
            document.body.style.overflow = 'hidden';
        }
    });
    
    // Close search when clicking outside
    document.addEventListener('click', function(e) {
        if (isSearchVisible && 
            !searchOverlay.contains(e.target) && 
            !searchToggleBtn.contains(e.target)) {
            
            searchOverlay.classList.remove('show');
            setTimeout(() => {
                searchOverlay.style.display = 'none';
            }, 400);
            isSearchVisible = false;
            
            // Change icon back to search
            searchToggleBtn.innerHTML = '<i class="fas fa-search"></i>';
            
            // Remove body scroll lock
            document.body.style.overflow = '';
        }
    });
    
    // Close search on escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && isSearchVisible) {
            searchOverlay.classList.remove('show');
            setTimeout(() => {
                searchOverlay.style.display = 'none';
            }, 400);
            isSearchVisible = false;
            
            // Change icon back to search
            searchToggleBtn.innerHTML = '<i class="fas fa-search"></i>';
            
            // Remove body scroll lock
            document.body.style.overflow = '';
        }
    });
}