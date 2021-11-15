/*!
* Start Bootstrap - Clean Blog v6.0.5 (https://startbootstrap.com/theme/clean-blog)
* Copyright 2013-2021 Start Bootstrap
* Licensed under MIT (https://github.com/StartBootstrap/startbootstrap-clean-blog/blob/master/LICENSE)
*/
window.addEventListener('DOMContentLoaded', () => {
    let scrollPos = 0;
    const navBar = document.getElementById('navBar');
    const headerHeight = navBar.clientHeight;
    window.addEventListener('scroll', function() {
        const currentTop = document.body.getBoundingClientRect().top * -1;
        if ( currentTop < scrollPos) {
            // Scrolling Up
            if (currentTop > 0 && navBar.classList.contains('is-fixed')) {
                navBar.classList.add('is-visible');
            } else {
                navBar.classList.remove('is-visible', 'is-fixed');
            }
        } else {
            // Scrolling Down
            navBar.classList.remove(['is-visible']);
            if (currentTop > headerHeight && !navBar.classList.contains('is-fixed')) {
                navBar.classList.add('is-fixed');
            }
        }
        scrollPos = currentTop;
    });
})
