// ============================================
// SmartBank Modern UI - JavaScript Enhancements
// ============================================

document.addEventListener("DOMContentLoaded", function () {
  // Initialize all enhancements
  initializeDarkMode();
  initializeAnimations();
  initializeCarousel();
  initializeSmoothScroll();
  initializeTooltips();
  initializeFormEnhancements();
});

// ============================================
// 1. DARK MODE TOGGLE
// ============================================
function initializeDarkMode() {
  const darkModeToggle = document.getElementById("darkModeToggle");
  if (!darkModeToggle) return;

  const html = document.documentElement;

  // Load dark mode preference from localStorage
  if (localStorage.getItem("darkMode") === "true") {
    document.body.classList.add("dark-mode");
    updateDarkModeIcon(true);
  }

  // Toggle dark mode on button click
  darkModeToggle.addEventListener("click", function () {
    document.body.classList.toggle("dark-mode");
    const isDarkMode = document.body.classList.contains("dark-mode");
    localStorage.setItem("darkMode", isDarkMode);
    updateDarkModeIcon(isDarkMode);
  });
}

function updateDarkModeIcon(isDarkMode) {
  const darkModeToggle = document.getElementById("darkModeToggle");
  if (darkModeToggle) {
    darkModeToggle.innerHTML = isDarkMode
      ? '<i class="bi bi-sun-fill"></i>'
      : '<i class="bi bi-moon-fill"></i>';
  }
}

// ============================================
// 2. SCROLL ANIMATIONS
// ============================================
function initializeAnimations() {
  // Observe elements for fade-in animation
  const observerOptions = {
    threshold: 0.1,
    rootMargin: "0px 0px -50px 0px",
  };

  const observer = new IntersectionObserver(function (entries) {
    entries.forEach((entry) => {
      if (entry.isIntersecting) {
        entry.target.classList.add("slide-up-animation");
        observer.unobserve(entry.target);
      }
    });
  }, observerOptions);

  // Observe cards, stats, and sections
  document
    .querySelectorAll(".card, .stat-card, .quick-actions-section")
    .forEach((el) => {
      observer.observe(el);
    });
}

// ============================================
// 3. ENHANCED CAROUSEL
// ============================================
function initializeCarousel() {
  const carouselElement = document.getElementById("dashboardCarousel");
  if (!carouselElement) return;

  const carousel = new bootstrap.Carousel(carouselElement, {
    interval: 5000,
    pause: "hover",
    ride: "carousel",
    wrap: true,
  });

  // Add smooth fade effect to carousel items
  const items = carouselElement.querySelectorAll(".carousel-item");
  items.forEach((item, index) => {
    item.style.transition = "opacity 1s ease-in-out";
    item.style.opacity = index === 0 ? "1" : "0";
  });

  // Enhanced carousel event listeners
  carouselElement.addEventListener("slide.bs.carousel", function (event) {
    const currentSlide = carouselElement.querySelector(".carousel-item.active");
    const nextSlide = event.relatedTarget;

    if (currentSlide) {
      currentSlide.style.opacity = "0";
    }
    nextSlide.style.opacity = "1";
  });
}

// ============================================
// 4. SMOOTH SCROLL
// ============================================
function initializeSmoothScroll() {
  document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
    anchor.addEventListener("click", function (e) {
      const href = this.getAttribute("href");
      if (href !== "#" && document.querySelector(href)) {
        e.preventDefault();
        const target = document.querySelector(href);
        target.scrollIntoView({
          behavior: "smooth",
          block: "start",
        });
      }
    });
  });
}

// ============================================
// 5. TOOLTIPS
// ============================================
function initializeTooltips() {
  // Initialize Bootstrap tooltips
  const tooltipTriggerList = [].slice.call(
    document.querySelectorAll('[data-bs-toggle="tooltip"]'),
  );
  tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
  });
}

// ============================================
// 6. FORM ENHANCEMENTS
// ============================================
function initializeFormEnhancements() {
  // Add focus animation to form inputs
  const inputs = document.querySelectorAll(".form-control, .form-select");

  inputs.forEach((input) => {
    // Add focus event listener
    input.addEventListener("focus", function () {
      this.parentElement.classList.add("focused");
      this.style.boxShadow = "0 0 0 0.2rem rgba(0, 94, 184, 0.15)";
    });

    // Add blur event listener
    input.addEventListener("blur", function () {
      this.parentElement.classList.remove("focused");
      this.style.boxShadow = "none";
    });

    // Add input validation feedback
    input.addEventListener("input", function () {
      validateInput(this);
    });
  });

  // Form submission handling
  const forms = document.querySelectorAll("form");
  forms.forEach((form) => {
    form.addEventListener("submit", function (e) {
      let isValid = true;

      this.querySelectorAll(".form-control, .form-select").forEach((input) => {
        if (!validateInput(input)) {
          isValid = false;
        }
      });

      if (!isValid) {
        e.preventDefault();
      }
    });
  });
}

function validateInput(input) {
  const value = input.value.trim();
  const type = input.type;
  let isValid = true;

  // Remove previous feedback classes
  input.classList.remove("is-valid", "is-invalid");

  // Validate based on input type
  if (input.hasAttribute("required") && !value) {
    isValid = false;
  }

  if (type === "email" && value && !isValidEmail(value)) {
    isValid = false;
  }

  if (type === "number" && value && isNaN(value)) {
    isValid = false;
  }

  // Add feedback class
  if (value) {
    input.classList.add(isValid ? "is-valid" : "is-invalid");
  }

  return isValid;
}

function isValidEmail(email) {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
}

// ============================================
// 7. UTILITY FUNCTIONS
// ============================================

// Add animation on page load
window.addEventListener("load", function () {
  document.body.style.animation = "pageLoad 0.3s ease-in";
});

// Handle active navigation link
function setActiveNavLink() {
  const currentLocation = window.location.href;
  const navLinks = document.querySelectorAll(".nav-link");

  navLinks.forEach((link) => {
    if (link.href === currentLocation) {
      link.classList.add("active");
    } else {
      link.classList.remove("active");
    }
  });
}

// Initialize active nav link on load
setActiveNavLink();

// ============================================
// 8. CARD HOVER EFFECTS
// ============================================
document.querySelectorAll(".card, .stat-card").forEach((card) => {
  card.addEventListener("mouseenter", function () {
    if (window.matchMedia("(min-width: 768px)").matches) {
      this.style.cursor = "pointer";
    }
  });
});

// ============================================
// 9. ALERT AUTO-DISMISS
// ============================================
document.addEventListener("DOMContentLoaded", function () {
  const alerts = document.querySelectorAll(
    ".alert-success-toast, .alert-dismissible",
  );

  alerts.forEach((alert) => {
    if (alert.classList.contains("alert-success-toast")) {
      setTimeout(function () {
        const bsAlert = new bootstrap.Alert(alert);
        bsAlert.close();
      }, 5000);
    }
  });
});

// ============================================
// 10. RESPONSIVE NAVIGATION
// ============================================
window.addEventListener("resize", function () {
  // Adjust layout on window resize
  const sidebar = document.querySelector(".sidebar");
  if (sidebar && window.innerWidth < 768) {
    sidebar.style.display = "none";
  }
});

// ============================================
// 11. UTILITY: Add ripple effect to buttons
// ============================================
function addRippleEffect() {
  const buttons = document.querySelectorAll("button, .btn, a.btn");

  buttons.forEach((btn) => {
    btn.addEventListener("click", function (e) {
      const ripple = document.createElement("span");
      const rect = this.getBoundingClientRect();
      const size = Math.max(rect.width, rect.height);
      const x = e.clientX - rect.left - size / 2;
      const y = e.clientY - rect.top - size / 2;

      ripple.style.width = ripple.style.height = size + "px";
      ripple.style.left = x + "px";
      ripple.style.top = y + "px";
      ripple.classList.add("ripple");

      this.appendChild(ripple);

      setTimeout(() => ripple.remove(), 600);
    });
  });
}

// Add ripple effect on DOMContentLoaded
document.addEventListener("DOMContentLoaded", addRippleEffect);

// ============================================
// 12. UTILITY: Format currency display
// ============================================
function formatCurrency(value) {
  return new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
  }).format(value);
}

// ============================================
// 13: UTILITY: Debounce function for resize events
// ============================================
function debounce(func, wait) {
  let timeout;
  return function executedFunction(...args) {
    const later = () => {
      clearTimeout(timeout);
      func(...args);
    };
    clearTimeout(timeout);
    timeout = setTimeout(later, wait);
  };
}

// ============================================
// 14: Performance optimizations
// ============================================
if ("loading" in HTMLImageElement.prototype) {
  // Use native lazy loading if available
  document.querySelectorAll("img").forEach((img) => {
    if (!img.hasAttribute("loading")) {
      img.setAttribute("loading", "lazy");
    }
  });
}

console.log("SmartBank Modern UI - JavaScript initialized successfully!");
