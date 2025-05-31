import React from 'react';
import { Backdrop, CircularProgress, Typography, Box } from '@mui/material';
import { useLoading } from '../contexts/LoadingContext';

const LoadingOverlay: React.FC = () => {
  const { isLoading, loadingText } = useLoading();

  return (
    <Backdrop
      sx={{
        color: '#fff',
        zIndex: (theme) => theme.zIndex.drawer + 1,
        display: 'flex',
        flexDirection: 'column',
        gap: 2,
      }}
      open={isLoading}
    >
      <CircularProgress color="inherit" />
      <Box sx={{ textAlign: 'center' }}>
        <Typography variant="h6">{loadingText}</Typography>
      </Box>
    </Backdrop>
  );
};

export default LoadingOverlay; 