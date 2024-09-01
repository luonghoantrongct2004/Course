$(function () {
  //search
  $(document).on("keydown", (e) => {
    switch (e.key) {
      case "k":
      case "Control":
        e.preventDefault();
        e.stopPropagation();
        break;
    }
    ``;
    if (e.key === "k" && e.ctrlKey) {
      $("#search").trigger("focus");
    }
  });
  //drawer
  $(".drawer-btn").on("click", () => {
    const checkClassExits = $(".layout-wrapper");
    if (checkClassExits.hasClass("active")) {
      checkClassExits.removeClass("active");
    } else {
      checkClassExits.addClass("active");
    }
  });
  //drawer key access
  $(document).on("keydown", (e) => {
    switch (e.key) {
      case "b":
      case "Control":
        e.preventDefault();
        e.stopPropagation();
        break;
    }
    if (e.key === "b" && e.ctrlKey) {
      const checkClassExits = $(".layout-wrapper");
      if (checkClassExits.hasClass("active")) {
        checkClassExits.removeClass("active");
      } else {
        checkClassExits.addClass("active");
      }
    }
  });
});

// Settings Tab
const tabs = document.querySelectorAll(".tab");
const tabContents = document.querySelectorAll(".tab-content");

tabs.forEach((tab) => {
  tab.addEventListener("click", () => {
    const tabId = tab.getAttribute("data-tab");

    tabs.forEach((t) => t.classList.remove("tab-active"));
    tab.classList.add("tab-active");

    tabContents.forEach((content) => {
      if (content.id === tabId) {
        content.classList.add("active");
      } else {
        content.classList.remove("active");
      }
    });
  });
});

// Switch Toggle
const switch_btn = document.querySelectorAll(".switch-btn");
if (switch_btn && switch_btn.length > 0) {
  switch_btn.forEach((item, index) => {
    item.addEventListener("click", () => {
      if (switch_btn[index].classList.contains("active")) {
        switch_btn[index].classList.remove("active");
      } else {
        switch_btn[index].classList.add("active");
      }
    });
  });
}

function notificationAction() {
  $("#notification-box").toggle();
  $("#noti-outside").toggle();
}
function wishlistAction() {
  $("#message-box").toggle();
  $("#mes-outside").toggle();
}
function storeAction() {
  $("#store-box").toggle();
  $("#store-outside").toggle();
}
function profileAction() {
  $(".profile-box").toggle();
  $(".profile-outside").toggle();
}
function toggleSettings() {
  $("#profile-settings").toggle();
}
function dropdownFilterAction(selector) {
  $(selector).toggle();
}

//navigation actions

function navSubmenu() {
  const navSelector = document.querySelector(".nav-wrapper");
  if (navSelector) {
    const navItems = navSelector.querySelectorAll(".item");
    if (navItems && navItems.length > 0) {
      navItems.forEach((item, i) => {
        const submenuExist = navItems[i].querySelector(".sub-menu");
        if (submenuExist) {
          const clickItem = navItems[i].querySelector("button");
          clickItem.addEventListener("click", (e) => {
            e.preventDefault();
            if (submenuExist.classList.contains("active")) {
              submenuExist.classList.remove("active");
              clickItem.classList.remove("bg-violet-50", "text-mainblue");
              clickItem.classList.add("text-mainblue");
            } else {
              submenuExist.classList.add("active");
              clickItem.classList.remove("text-slate-700");
              clickItem.classList.add("bg-violet-50", "text-mainblue");
            }
          });
        } else {
          return false;
        }
      });
    } else {
      return false;
    }
  } else {
    return false;
  }
}
navSubmenu();

// Modal
function modalExist() {
  const openModalButtons = document.querySelectorAll("[data-modal-target]");
  const closeModalButtons = document.querySelectorAll("[data-close-modal]");
  const overlay = document.getElementById("overlay");
  if (openModalButtons) {
    openModalButtons.forEach((button) => {
      button.addEventListener("click", () => {
        const modal = document.querySelector(button.dataset.modalTarget);
        openModal(modal);
      });
    });

    closeModalButtons.forEach((button) => {
      button.addEventListener("click", () => {
        const modal = button.closest(".modal");
        closeModal(modal);
      });
    });

    overlay.addEventListener("click", () => {
      const modals = document.querySelectorAll(".modal");
      modals.forEach((modal) => {
        closeModal(modal);
      });
    });

    function openModal(modal) {
      if (modal == null) return;
      modal.classList.remove("hidden");
      overlay.classList.remove("hidden");
    }

    function closeModal(modal) {
      if (modal == null) return;
      modal.classList.add("hidden");
      overlay.classList.add("hidden");
    }
  }
}
modalExist();
