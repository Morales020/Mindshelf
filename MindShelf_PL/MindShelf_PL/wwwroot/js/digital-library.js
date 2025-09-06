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

    if (!searchInput || !searchSuggestions) return;

    searchInput.addEventListener('input', function () {
        const query = this.value.trim();

        // مسح المؤقت السابق
        clearTimeout(searchTimeout);

        if (query.length < 2) {
            hideSuggestions();
            return;
        }

        // تأخير البحث لتحسين الأداء
        searchTimeout = setTimeout(() => {
            performSearch(query);
        }, 300);
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

    try {
        // عرض مؤشر التحميل
        searchSuggestions.innerHTML = '<div class="search-suggestion-item">جاري البحث...</div>';
        searchSuggestions.style.display = 'block';

        // محاكاة البحث (يجب استبدالها بـ API حقيقي)
        const suggestions = await fetchSearchSuggestions(query);

        displaySuggestions(suggestions);

    } catch (error) {
        console.error('خطأ في البحث:', error);
        searchSuggestions.innerHTML = '<div class="search-suggestion-item text-danger">حدث خطأ في البحث</div>';
    }
}

async function fetchSearchSuggestions(query) {
    // محاكاة بيانات البحث (يجب استبدالها بـ API حقيقي)
    const mockSuggestions = [
        { type: 'book', title: 'الأسود يليق بك', author: 'أحلام مستغانمي', id: 1 },
        { type: 'book', title: 'مئة عام من العزلة', author: 'غابرييل غارسيا ماركيز', id: 2 },
        { type: 'author', name: 'نجيب محفوظ', booksCount: 45, id: 1 },
        { type: 'author', name: 'أحمد شوقي', booksCount: 23, id: 2 },
        { type: 'category', name: 'الأدب العربي', booksCount: 156, id: 1 }
    ];

    // تأخير للمحاكاة
    await new Promise(resolve => setTimeout(resolve, 200));

    return mockSuggestions.filter(item =>
        (item.title && item.title.includes(query)) ||
        (item.name && item.name.includes(query)) ||
        (item.author && item.author.includes(query))
    ).slice(0, 5);
}

function displaySuggestions(suggestions) {
    const searchSuggestions = document.getElementById('searchSuggestions');

    if (suggestions.length === 0) {
        searchSuggestions.innerHTML = '<div class="search-suggestion-item">لا توجد نتائج</div>';
        return;
    }

    const html = suggestions.map(item => {
        if (item.type === 'book') {
            return `
                <div class="search-suggestion-item" onclick="goToBook(${item.id})">
                    <i class="fas fa-book me-2 text-primary"></i>
                    <strong>${item.title}</strong>
                    <br>
                    <small class="text-muted">بقلم: ${item.author}</small>
                </div>
            `;
        } else if (item.type === 'author') {
            return `
                <div class="search-suggestion-item" onclick="goToAuthor(${item.id})">
                    <i class="fas fa-user-edit me-2 text-brown"></i>
                    <strong>${item.name}</strong>
                    <br>
                    <small class="text-muted">${item.booksCount} كتاب</small>
                </div>
            `;
        } else if (item.type === 'category') {
            return `
                <div class="search-suggestion-item" onclick="goToCategory(${item.id})">
                    <i class="fas fa-list me-2 text-info"></i>
                    <strong>${item.name}</strong>
                    <br>
                    <small class="text-muted">${item.booksCount} كتاب</small>
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
    // إنشاء عنصر الإشعار
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
    notification.style.cssText = `
        top: 20px;
        left: 20px;
        z-index: 9999;
        max-width: 400px;
        box-shadow: var(--box-shadow);
    `;

    const iconMap = {
        'success': 'fas fa-check-circle',
        'warning': 'fas fa-exclamation-triangle',
        'danger': 'fas fa-exclamation-circle',
        'info': 'fas fa-info-circle'
    };

    notification.innerHTML = `
        <i class="${iconMap[type] || iconMap.info}"></i>
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;

    document.body.appendChild(notification);

    // إزالة الإشعار تلقائياً
    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
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
        testSearch: (query) => performSearch(query)
    };

    console.log('🚀 MindShelf Debug Mode Active');
    console.log('Available commands: debug.isDarkMode(), debug.showAllNotifications(), debug.testSearch(query)');
}